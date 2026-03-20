namespace SSD.Infrastructure.Spotify;

public interface ISpotifyApiClient
{
    Task<SpotifyTokenResponse> ExchangeCodeAsync(string code, string codeVerifier, CancellationToken cancellationToken);

    Task<SpotifyTokenResponse> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken);

    Task<SpotifyTokenResponse> GetClientCredentialsTokenAsync(CancellationToken cancellationToken);

    Task<SpotifyCurrentUserResponse> GetCurrentUserAsync(string accessToken, CancellationToken cancellationToken);

    Task<SpotifyTrackApiResponse> GetTrackAsync(string trackId, string accessToken, CancellationToken cancellationToken);

    Task<SpotifyArtistApiResponse> GetArtistAsync(string artistId, string accessToken, CancellationToken cancellationToken);

    Task<SpotifyPlaylistApiResponse> GetPlaylistAsync(string playlistId, string accessToken, CancellationToken cancellationToken);

    Task<SpotifyPagingResponse<SpotifyTrackApiResponse>> GetCurrentUserTopTracksAsync(string accessToken, int limit, CancellationToken cancellationToken);

    Task<SpotifyPagingResponse<SpotifyArtistApiResponse>> GetCurrentUserTopArtistsAsync(string accessToken, int limit, CancellationToken cancellationToken);
}
