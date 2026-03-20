using SSD.Application.Contracts.Spotify;

namespace SSD.Application.Abstractions;

public interface ISpotifyService
{
    Task<SpotifyAuthorizationStartResponse> CreateLinkStartAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<SpotifyLinkResult> CompleteLinkAsync(string code, string state, CancellationToken cancellationToken = default);

    Task<SpotifyTrackResponse> ResolveTrackAsync(string spotifyLinkOrUri, CancellationToken cancellationToken = default);

    Task<SpotifyLinkedAccountResponse> GetLinkedAccountAsync(Guid userId, CancellationToken cancellationToken = default);
}
