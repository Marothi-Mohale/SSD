namespace SSD.Infrastructure.Spotify;

public interface ISpotifyApiClient
{
    Task<SpotifyTokenResponse> ExchangeCodeAsync(string code, CancellationToken cancellationToken);

    Task<SpotifyTokenResponse> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken);

    Task<SpotifyTokenResponse> GetClientCredentialsTokenAsync(CancellationToken cancellationToken);

    Task<SpotifyCurrentUserResponse> GetCurrentUserAsync(string accessToken, CancellationToken cancellationToken);

    Task<SpotifyTrackApiResponse> GetTrackAsync(string trackId, string accessToken, CancellationToken cancellationToken);
}
