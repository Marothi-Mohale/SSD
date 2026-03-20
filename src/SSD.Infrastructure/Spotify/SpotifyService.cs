#pragma warning disable CA1848
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSD.Application.Abstractions;
using SSD.Application.Contracts.Spotify;
using SSD.Application.Exceptions;
using SSD.Domain.Entities;
using SSD.Domain.Enums;
using SSD.Infrastructure.Persistence;

namespace SSD.Infrastructure.Spotify;

internal sealed class SpotifyService(
    SsdDbContext dbContext,
    ISpotifyApiClient spotifyApiClient,
    ISpotifyTokenProtector tokenProtector,
    SpotifyOAuthStateProtector stateProtector,
    IMoodRuleCatalog moodRuleCatalog,
    IOptions<SpotifyOptions> options,
    ILogger<SpotifyService> logger) : ISpotifyService
{
    private readonly SpotifyOptions _options = options.Value;

    public Task<SpotifyAuthorizationStartResponse> CreateLinkStartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var codeVerifier = CreateCodeVerifier();
        var state = stateProtector.Protect(userId, codeVerifier);
        var codeChallenge = CreateCodeChallenge(codeVerifier);
        var authorizationUrl =
            $"{_options.AuthorizationBaseUrl}?client_id={Uri.EscapeDataString(_options.ClientId)}&response_type=code&redirect_uri={Uri.EscapeDataString(_options.RedirectUri)}&scope={Uri.EscapeDataString(_options.Scopes)}&state={Uri.EscapeDataString(state)}&code_challenge_method=S256&code_challenge={Uri.EscapeDataString(codeChallenge)}";

        return Task.FromResult(new SpotifyAuthorizationStartResponse(authorizationUrl, state));
    }

    public async Task<SpotifyLinkResult> CompleteLinkAsync(string code, string state, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new IntegrationException("spotify_code_missing", "Spotify did not return an authorization code.");
        }

        Guid userId;
        string codeVerifier;
        try
        {
            var oauthState = stateProtector.Unprotect(state, TimeSpan.FromMinutes(10));
            userId = oauthState.UserId;
            codeVerifier = oauthState.CodeVerifier;
        }
        catch (Exception)
        {
            throw new IntegrationException("spotify_state_invalid", "The Spotify authorization state is invalid.", 400);
        }

        var user = await dbContext.Users.SingleOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken)
            ?? throw new IntegrationException("spotify_user_missing", "The SSD user for this Spotify link could not be found.", 404);

        var tokenResponse = await spotifyApiClient.ExchangeCodeAsync(code, codeVerifier, cancellationToken);
        if (string.IsNullOrWhiteSpace(tokenResponse.RefreshToken))
        {
            throw new IntegrationException("spotify_refresh_token_missing", "Spotify did not return a refresh token for this authorization.", 502);
        }

        var profile = await spotifyApiClient.GetCurrentUserAsync(tokenResponse.AccessToken, cancellationToken);

        var linkedAccount = await dbContext.LinkedSpotifyAccounts.SingleOrDefaultAsync(candidate => candidate.UserId == userId, cancellationToken);
        if (linkedAccount is null)
        {
            linkedAccount = new LinkedSpotifyAccount(Guid.NewGuid(), userId, profile.Id, tokenProtector.Protect(tokenResponse.RefreshToken));
            dbContext.LinkedSpotifyAccounts.Add(linkedAccount);
        }

        linkedAccount.UpdateTokens(
            tokenProtector.Protect(tokenResponse.RefreshToken),
            tokenProtector.Protect(tokenResponse.AccessToken),
            DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn));
        linkedAccount.UpdateProfile(
            profile.Id,
            profile.DisplayName,
            profile.Email,
            profile.Country,
            profile.Product,
            (tokenResponse.Scope ?? _options.Scopes).Split(' ', StringSplitOptions.RemoveEmptyEntries));

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Linked Spotify account {SpotifyUserId} to SSD user {UserId}", profile.Id, userId);

        return new SpotifyLinkResult(
            userId,
            profile.Id,
            profile.DisplayName,
            profile.Email,
            linkedAccount.Status.ToString(),
            linkedAccount.LinkedUtc);
    }

    public async Task<SpotifyTrackResponse> ResolveTrackAsync(string spotifyLinkOrUri, CancellationToken cancellationToken = default)
    {
        var trackId = SpotifyUrlParser.ParseTrackId(spotifyLinkOrUri);
        var token = await spotifyApiClient.GetClientCredentialsTokenAsync(cancellationToken);
        var track = await spotifyApiClient.GetTrackAsync(trackId, token.AccessToken, cancellationToken);

        if (!track.IsPlayable.GetValueOrDefault(true))
        {
            throw new IntegrationException("spotify_track_unavailable", "The Spotify track is unavailable for playback in the current market.", 409);
        }

        return SpotifyMapper.MapTrack(track);
    }

    public async Task<SpotifyArtistResponse> ResolveArtistAsync(string spotifyLinkOrUri, CancellationToken cancellationToken = default)
    {
        var artistId = SpotifyUrlParser.ParseArtistId(spotifyLinkOrUri);
        var token = await spotifyApiClient.GetClientCredentialsTokenAsync(cancellationToken);
        var artist = await spotifyApiClient.GetArtistAsync(artistId, token.AccessToken, cancellationToken);
        return SpotifyMapper.MapArtist(artist);
    }

    public async Task<SpotifyPlaylistResponse> ResolvePlaylistAsync(Guid? userId, string spotifyLinkOrUri, CancellationToken cancellationToken = default)
    {
        var playlistId = SpotifyUrlParser.ParsePlaylistId(spotifyLinkOrUri);
        var accessToken = await GetPlaylistAccessTokenAsync(userId, cancellationToken);
        var playlist = await spotifyApiClient.GetPlaylistAsync(playlistId, accessToken, cancellationToken);
        return SpotifyMapper.MapPlaylist(playlist);
    }

    public async Task<SpotifyLinkedAccountResponse> GetLinkedAccountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var account = await dbContext.LinkedSpotifyAccounts.SingleOrDefaultAsync(candidate => candidate.UserId == userId, cancellationToken);
        if (account is null || account.Status != SpotifyLinkStatus.Active)
        {
            return new SpotifyLinkedAccountResponse(false, null, null, null, null, null, [], null, null);
        }

        var accessToken = await GetValidUserAccessTokenAsync(account, cancellationToken);
        var profile = await spotifyApiClient.GetCurrentUserAsync(accessToken, cancellationToken);

        account.UpdateProfile(account.SpotifyUserId, profile.DisplayName, profile.Email, profile.Country, profile.Product, account.GrantedScopes);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new SpotifyLinkedAccountResponse(
            true,
            account.SpotifyUserId,
            profile.DisplayName,
            profile.Email,
            profile.Country,
            profile.Product,
            account.GrantedScopes,
            account.LinkedUtc,
            account.LastSyncedUtc);
    }

    public async Task<SpotifyRecommendationContextResponse> GetRecommendationContextAsync(Guid userId, string? mood, CancellationToken cancellationToken = default)
    {
        var account = await dbContext.LinkedSpotifyAccounts.SingleOrDefaultAsync(candidate => candidate.UserId == userId, cancellationToken);
        var normalizedMood = NormalizeMood(mood);
        if (account is null || account.Status != SpotifyLinkStatus.Active)
        {
            return new SpotifyRecommendationContextResponse(
                false,
                normalizedMood?.ToString(),
                [],
                [],
                [],
                [],
                "Link Spotify to use listening history as an additional signal for music recommendations.",
                []);
        }

        if (!account.GrantedScopes.Contains("user-top-read", StringComparer.Ordinal))
        {
            return new SpotifyRecommendationContextResponse(
                true,
                normalizedMood?.ToString(),
                account.GrantedScopes,
                [],
                [],
                [],
                "Spotify is linked, but SSD needs the user-top-read scope to use top artists and tracks as recommendation seeds.",
                ["Re-link Spotify with top-listening access to improve recommendation quality."]);
        }

        var accessToken = await GetValidUserAccessTokenAsync(account, cancellationToken);
        var topTracks = await spotifyApiClient.GetCurrentUserTopTracksAsync(accessToken, _options.RecommendationTopTrackLimit, cancellationToken);
        var topArtists = await spotifyApiClient.GetCurrentUserTopArtistsAsync(accessToken, _options.RecommendationTopArtistLimit, cancellationToken);
        var moodRule = normalizedMood is null ? null : moodRuleCatalog.GetRule(normalizedMood.Value);

        return SpotifyRecommendationContextBuilder.Build(
            normalizedMood?.ToString(),
            account.GrantedScopes,
            topArtists.Items.Select(SpotifyMapper.MapArtist).ToArray(),
            topTracks.Items.Select(SpotifyMapper.MapTrackSummary).ToArray(),
            moodRule);
    }

    private async Task<string> GetPlaylistAccessTokenAsync(Guid? userId, CancellationToken cancellationToken)
    {
        if (userId is null)
        {
            var appToken = await spotifyApiClient.GetClientCredentialsTokenAsync(cancellationToken);
            return appToken.AccessToken;
        }

        var account = await dbContext.LinkedSpotifyAccounts.SingleOrDefaultAsync(candidate => candidate.UserId == userId, cancellationToken);
        if (account is null || account.Status != SpotifyLinkStatus.Active)
        {
            var appToken = await spotifyApiClient.GetClientCredentialsTokenAsync(cancellationToken);
            return appToken.AccessToken;
        }

        return await GetValidUserAccessTokenAsync(account, cancellationToken);
    }

    private async Task<string> GetValidUserAccessTokenAsync(LinkedSpotifyAccount account, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(account.EncryptedAccessToken) &&
            account.AccessTokenExpiresUtc is { } expiresUtc &&
            expiresUtc > DateTimeOffset.UtcNow.AddMinutes(1))
        {
            return tokenProtector.Unprotect(account.EncryptedAccessToken);
        }

        try
        {
            var refreshed = await spotifyApiClient.RefreshAccessTokenAsync(tokenProtector.Unprotect(account.EncryptedRefreshToken), cancellationToken);
            var effectiveRefreshToken = string.IsNullOrWhiteSpace(refreshed.RefreshToken)
                ? tokenProtector.Unprotect(account.EncryptedRefreshToken)
                : refreshed.RefreshToken;

            account.UpdateTokens(
                tokenProtector.Protect(effectiveRefreshToken),
                tokenProtector.Protect(refreshed.AccessToken),
                DateTimeOffset.UtcNow.AddSeconds(refreshed.ExpiresIn));

            return refreshed.AccessToken;
        }
        catch (IntegrationException exception) when (exception.Code == "spotify_token_error")
        {
            account.MarkRevoked();
            await dbContext.SaveChangesAsync(cancellationToken);
            throw new IntegrationException("spotify_link_revoked", "The linked Spotify account needs to be reconnected.", (int)HttpStatusCode.Unauthorized);
        }
    }

    private static string CreateCodeVerifier()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Base64UrlEncode(bytes);
    }

    private static string CreateCodeChallenge(string codeVerifier)
    {
        var hash = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(ReadOnlySpan<byte> value)
    {
        return Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static MoodCategory? NormalizeMood(string? mood)
    {
        if (string.IsNullOrWhiteSpace(mood))
        {
            return null;
        }

        var normalized = mood.Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal);

        foreach (var candidate in Enum.GetValues<MoodCategory>())
        {
            if (string.Equals(candidate.ToString(), normalized, StringComparison.OrdinalIgnoreCase))
            {
                return candidate;
            }
        }

        return null;
    }
}
#pragma warning restore CA1848
