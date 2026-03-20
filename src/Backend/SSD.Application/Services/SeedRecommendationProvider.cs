using SSD.Application.Abstractions;
using SSD.Domain.Entities;
using SSD.Domain.Enums;
using SSD.Domain.ValueObjects;

namespace SSD.Application.Services;

public sealed class SeedRecommendationProvider : IRecommendationProvider
{
    private static readonly IReadOnlyList<ContentRecommendationSeed> Seeds =
    [
        new(
            "music-midnight-city",
            RecommendationKind.Music,
            MoodCategory.Energetic,
            "Midnight City",
            "M83",
            "Synth-pop",
            "SpotifySeed",
            "A neon-lit anthem that turns restless energy into momentum.",
            0.95m,
            ["Up-tempo synth layers", "Great for a late-night energy lift", "Pairs well with adventurous moods"]),
        new(
            "music-weightless",
            RecommendationKind.Music,
            MoodCategory.Calm,
            "Weightless",
            "Marconi Union",
            "Ambient",
            "SpotifySeed",
            "A spacious ambient track suited for winding down.",
            0.96m,
            ["Soft texture", "Low intensity", "Ideal for calm moods"]),
        new(
            "music-time",
            RecommendationKind.Music,
            MoodCategory.Nostalgic,
            "Time",
            "Pink Floyd",
            "Progressive Rock",
            "SpotifySeed",
            "A reflective classic with emotional lift and familiarity.",
            0.92m,
            ["Reflective lyrics", "Classic sound", "Strong nostalgia signal"]),
        new(
            "music-lofi-girl",
            RecommendationKind.Music,
            MoodCategory.Focused,
            "Lofi Study Session",
            "Various Artists",
            "Lo-fi",
            "SpotifySeed",
            "Steady beats designed to help the mind settle into deep work.",
            0.94m,
            ["Low distraction", "Consistent tempo", "Works well for focus"]),
        new(
            "movie-secret-life-of-walter-mitty",
            RecommendationKind.Movie,
            MoodCategory.Adventurous,
            "The Secret Life of Walter Mitty",
            "Ben Stiller",
            "Adventure / Comedy",
            "TMDbSeed",
            "A hopeful journey that matches curiosity, wonder, and movement.",
            0.95m,
            ["Travel-forward story", "Optimistic tone", "Adventure mood fit"]),
        new(
            "movie-paddington-2",
            RecommendationKind.Movie,
            MoodCategory.Cozy,
            "Paddington 2",
            "Paul King",
            "Family / Comedy",
            "TMDbSeed",
            "A warm, funny, family-friendly pick for comfort viewing.",
            0.97m,
            ["Gentle emotional tone", "Family friendly", "Comfort-first energy"]),
        new(
            "movie-arrival",
            RecommendationKind.Movie,
            MoodCategory.Focused,
            "Arrival",
            "Denis Villeneuve",
            "Sci-Fi / Drama",
            "TMDbSeed",
            "Thoughtful and immersive for users wanting a more cerebral match.",
            0.91m,
            ["Slow-burn pacing", "Thoughtful narrative", "Focus-friendly tone"]),
        new(
            "movie-about-time",
            RecommendationKind.Movie,
            MoodCategory.Nostalgic,
            "About Time",
            "Richard Curtis",
            "Romance / Drama",
            "TMDbSeed",
            "A sentimental recommendation for reflective and nostalgic moods.",
            0.93m,
            ["Emotional warmth", "Reflective themes", "Strong nostalgia fit"])
    ];

    public Task<IReadOnlyList<ContentRecommendation>> GetRecommendationsAsync(
        MoodSelection selection,
        CancellationToken cancellationToken = default)
    {
        var filtered = Seeds
            .Where(seed => seed.Mood == selection.Mood)
            .Where(seed => selection.IncludeMusic || seed.Kind != RecommendationKind.Music)
            .Where(seed => selection.IncludeMovies || seed.Kind != RecommendationKind.Movie)
            .Where(seed => !selection.FamilyFriendlyOnly || seed.Kind != RecommendationKind.Movie || seed.Title == "Paddington 2")
            .Select(seed => seed.ToRecommendation(selection))
            .ToArray();

        return Task.FromResult<IReadOnlyList<ContentRecommendation>>(filtered);
    }

    private sealed record ContentRecommendationSeed(
        string Id,
        RecommendationKind Kind,
        MoodCategory Mood,
        string Title,
        string Creator,
        string Genre,
        string Provider,
        string Description,
        decimal BaseScore,
        IReadOnlyList<string> Signals)
    {
        public ContentRecommendation ToRecommendation(MoodSelection selection)
        {
            var score = BaseScore;
            var why = Signals.ToList();

            if (!string.IsNullOrWhiteSpace(selection.Energy))
            {
                why.Add($"Energy filter: {selection.Energy}");
                score += selection.Energy.Equals("high", StringComparison.OrdinalIgnoreCase) && selection.Mood == MoodCategory.Energetic
                    ? 0.02m
                    : 0m;
            }

            if (!string.IsNullOrWhiteSpace(selection.TimeOfDay))
            {
                why.Add($"Time of day: {selection.TimeOfDay}");
            }

            return new ContentRecommendation(
                Id,
                Kind,
                Title,
                Creator,
                Genre,
                Provider,
                Description,
                Math.Min(score, 0.99m),
                new RecommendationReason(
                    $"This {Kind.ToString().ToLowerInvariant()} matches a {selection.Mood.ToString().ToLowerInvariant()} mood.",
                    why));
        }
    }
}
