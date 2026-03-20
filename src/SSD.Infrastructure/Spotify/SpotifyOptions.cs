namespace SSD.Infrastructure.Spotify;

public sealed class SpotifyOptions
{
    public const string SectionName = "Providers:Spotify";

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string RedirectUri { get; set; } = string.Empty;

    public string AuthorizationBaseUrl { get; set; } = "https://accounts.spotify.com/authorize";

    public string TokenBaseUrl { get; set; } = "https://accounts.spotify.com/api/token";

    public string ApiBaseUrl { get; set; } = "https://api.spotify.com/v1/";

    public string Scopes { get; set; } = "user-read-email user-read-private user-top-read";

    public int RecommendationTopTrackLimit { get; set; } = 5;

    public int RecommendationTopArtistLimit { get; set; } = 5;
}
