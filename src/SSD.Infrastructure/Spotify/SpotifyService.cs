#pragma warning disable CA1848
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSD.Application.Abstractions;
using SSD.Application.Contracts.Spotify;
using SSD.Application.Exceptions;
using SSD.Domain.Entities;
using SSD.Infrastructure.Persistence;

namespace SSD.Infrastructure.Spotify;

internal sealed class SpotifyService(
    SsdDbContext dbContext,
    ISpotifyApiClient spotifyApiClient,
    ISpotifyTokenProtector tokenProtector,
    SpotifyOAuthStateProtector stateProtector,
    IOptions<SpotifyOptions> options,
    ILogger<SpotifyService> logger) : ISpotifyService
{
    private readonly SpotifyOptions _options = options.Value;

    public Task<SpotifyAuthorizationStartResponse> CreateLinkStartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var state = stateProtector.Protect(userId);
        var authorizationUrl =
            $"{_options.AuthorizationBaseUrl}?client_id={Uri.EscapeDataString(_options.ClientId)}&response_type=code&redirect_uri={Uri.EscapeDataString(_options.RedirectUri)}&scope={Uri.EscapeDataString(_options.Scopes)}&state={Uri.EscapeDataString(state)}";

        return Task.FromResult(new SpotifyAuthorizationStartResponse(authorizationUrl, state));
    }

    public async Task<SpotifyLinkResult> CompleteLinkAsync(string code, string state, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new IntegrationException("spotify_code_missing", "Spotify did not return an authorization code.");
        }

        Guid userId;
        try
        {
            userId = stateProtector.Unprotect(state, TimeSpan.FromMinutes(10));
        }
        catch (Exception)
        {
            throw new IntegrationException("spotify_state_invalid", "The Spotify authorization state is invalid.", 400);
        }

        var user = await dbContext.Users.SingleOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken)
            ?? throw new IntegrationException("spotify_user_missing", "The SSD user for this Spotify link could not be found.", 404);

        var tokenResponse = await spotifyApiClient.ExchangeCodeAsync(code, cancellationToken);
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

    public async Task<SpotifyLinkedAccountResponse> GetLinkedAccountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var account = await dbContext.LinkedSpotifyAccounts.SingleOrDefaultAsync(candidate => candidate.UserId == userId, cancellationToken);
        if (account is null || account.Status != SSD.Domain.Enums.SpotifyLinkStatus.Active)
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
}
#pragma warning restore CA1848
