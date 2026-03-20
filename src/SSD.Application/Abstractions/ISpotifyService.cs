using SSD.Application.Contracts.Spotify;

namespace SSD.Application.Abstractions;

public interface ISpotifyService
{
    Task<SpotifyAuthorizationStartResponse> CreateLinkStartAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<SpotifyLinkResult> CompleteLinkAsync(string code, string state, CancellationToken cancellationToken = default);

    Task<SpotifyTrackResponse> ResolveTrackAsync(string spotifyLinkOrUri, CancellationToken cancellationToken = default);

    Task<SpotifyArtistResponse> ResolveArtistAsync(string spotifyLinkOrUri, CancellationToken cancellationToken = default);

    Task<SpotifyPlaylistResponse> ResolvePlaylistAsync(Guid? userId, string spotifyLinkOrUri, CancellationToken cancellationToken = default);

    Task<SpotifyLinkedAccountResponse> GetLinkedAccountAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<SpotifyRecommendationContextResponse> GetRecommendationContextAsync(Guid userId, string? mood, CancellationToken cancellationToken = default);
}
