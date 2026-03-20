namespace SSD.Application.Contracts.Spotify;

public sealed record SpotifyRecommendationContextResponse(
    bool IsLinked,
    string? Mood,
    IReadOnlyList<string> GrantedScopes,
    IReadOnlyList<string> SeedGenres,
    IReadOnlyList<SpotifyArtistResponse> TopArtists,
    IReadOnlyList<SpotifyTrackSummaryResponse> TopTracks,
    string Explanation,
    IReadOnlyList<string> Signals);
