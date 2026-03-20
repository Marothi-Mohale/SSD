using SSD.Application.Contracts.Spotify;
using SSD.Domain.Moods;

namespace SSD.Infrastructure.Spotify;

internal static class SpotifyRecommendationContextBuilder
{
    public static SpotifyRecommendationContextResponse Build(
        string? mood,
        IReadOnlyList<string> grantedScopes,
        IReadOnlyList<SpotifyArtistResponse> topArtists,
        IReadOnlyList<SpotifyTrackSummaryResponse> topTracks,
        MoodRuleDefinition? moodRule)
    {
        var seedGenres = topArtists
            .SelectMany(artist => artist.Genres)
            .Where(genre => !string.IsNullOrWhiteSpace(genre))
            .Select(Normalize)
            .Distinct(StringComparer.Ordinal)
            .Take(8)
            .ToArray();

        var signals = new List<string>();
        if (moodRule is not null)
        {
            foreach (var matchedGenre in seedGenres.Where(genre => MatchesMoodGenre(genre, moodRule.Music.FavoredGenres.Select(x => x.Key))).Take(4))
            {
                signals.Add($"Spotify listening leans toward {matchedGenre}, which aligns with the {moodRule.DisplayName} music profile.");
            }

            foreach (var artist in topArtists.Take(2))
            {
                signals.Add($"Recent listening includes {artist.Name}, which provides a reliable seed for {moodRule.DisplayName.ToLowerInvariant()} recommendations.");
            }
        }
        else
        {
            foreach (var artist in topArtists.Take(3))
            {
                signals.Add($"Top artist signal: {artist.Name}.");
            }
        }

        foreach (var track in topTracks.Take(2))
        {
            signals.Add($"Top track signal: {track.Title}.");
        }

        var explanation = BuildExplanation(mood, topArtists, topTracks, seedGenres, moodRule);

        return new SpotifyRecommendationContextResponse(
            true,
            mood,
            grantedScopes,
            seedGenres,
            topArtists,
            topTracks,
            explanation,
            signals.Distinct(StringComparer.Ordinal).ToArray());
    }

    private static string BuildExplanation(
        string? mood,
        IReadOnlyList<SpotifyArtistResponse> topArtists,
        IReadOnlyList<SpotifyTrackSummaryResponse> topTracks,
        string[] seedGenres,
        MoodRuleDefinition? moodRule)
    {
        _ = mood;

        if (moodRule is not null)
        {
            var genreSummary = seedGenres.Length > 0
                ? $"seed genres such as {string.Join(", ", seedGenres.Take(3))}"
                : "your recent Spotify listening patterns";
            var artistSummary = topArtists.Count > 0
                ? $"artists like {string.Join(", ", topArtists.Take(2).Select(artist => artist.Name))}"
                : "your linked Spotify profile";

            return $"SSD can bias {moodRule.DisplayName.ToLowerInvariant()} music recommendations using {artistSummary} and {genreSummary}.";
        }

        if (topArtists.Count > 0 || topTracks.Count > 0)
        {
            return "SSD can use your linked Spotify listening profile as an additional deterministic seed when building music recommendations.";
        }

        return "Your Spotify account is linked, but there is not enough top-listening data yet to influence recommendations.";
    }

    private static bool MatchesMoodGenre(string genre, IEnumerable<string> favoredGenres)
    {
        return favoredGenres
            .Select(Normalize)
            .Any(favored => genre.Contains(favored, StringComparison.Ordinal) || favored.Contains(genre, StringComparison.Ordinal));
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToLowerInvariant();
    }
}
