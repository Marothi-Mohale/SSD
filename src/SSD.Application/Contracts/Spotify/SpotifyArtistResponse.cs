namespace SSD.Application.Contracts.Spotify;

public sealed record SpotifyArtistResponse(
    string SpotifyId,
    string Name,
    IReadOnlyList<string> Genres,
    int Popularity,
    int Followers,
    string? ArtworkUrl,
    string ExternalUrl,
    string Provider = "Spotify");
