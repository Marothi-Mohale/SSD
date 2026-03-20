using SSD.Application.Abstractions;
using SSD.Domain.Entities;
using SSD.Domain.Enums;
using SSD.Domain.ValueObjects;

namespace SSD.Infrastructure.Recommendations;

public sealed class SeedRecommendationProvider : IRecommendationProvider
{
    private static readonly IReadOnlyList<RecommendationSeed> Seeds =
    [
        new(
            "music-weightless",
            RecommendationKind.Music,
            MoodCategory.Calm,
            "Weightless",
            "Marconi Union",
            "Ambient",
            "Spotify",
            "Gentle ambient textures for slowing down and decompressing.",
            0.96m,
            ["Low intensity soundscape", "Consistent pacing", "Excellent fit for calm moods"]),
        new(
            "music-midnight-city",
            RecommendationKind.Music,
            MoodCategory.Energetic,
            "Midnight City",
            "M83",
            "Synth-pop",
            "Spotify",
            "A high-energy synth anthem that gives the mood immediate momentum.",
            0.95m,
            ["Fast tempo", "Bright synth layers", "Late-evening energy fit"]),
        new(
            "music-lofi-session",
            RecommendationKind.Music,
            MoodCategory.Focused,
            "Lofi Study Session",
            "Various Artists",
            "Lo-fi",
            "Spotify",
            "Steady instrumental beats designed to help attention settle.",
            0.94m,
            ["Low distraction", "Predictable rhythm", "Strong focus signal"]),
        new(
            "music-time",
            RecommendationKind.Music,
            MoodCategory.Nostalgic,
            "Time",
            "Pink Floyd",
            "Progressive Rock",
            "Spotify",
            "A reflective classic for users leaning toward memory and introspection.",
            0.92m,
            ["Classic sound", "Reflective tone", "Nostalgia-heavy mood fit"]),
        new(
            "movie-walter-mitty",
            RecommendationKind.Movie,
            MoodCategory.Adventurous,
            "The Secret Life of Walter Mitty",
            "Ben Stiller",
            "Adventure / Comedy",
            "TMDb",
            "A hopeful travel-forward movie for curiosity, movement, and discovery.",
            0.95m,
            ["Adventure-first story", "Optimistic tone", "Matches exploration"]),
        new(
            "movie-paddington-2",
            RecommendationKind.Movie,
            MoodCategory.Cozy,
            "Paddington 2",
            "Paul King",
            "Family / Comedy",
            "TMDb",
            "A warm, funny comfort-watch with strong family-friendly appeal.",
            0.97m,
            ["Gentle humor", "Warm emotional tone", "Family friendly"]),
        new(
            "movie-arrival",
            RecommendationKind.Movie,
            MoodCategory.Focused,
            "Arrival",
            "Denis Villeneuve",
            "Sci-Fi / Drama",
            "TMDb",
            "A thoughtful and immersive film for concentrated, cerebral viewing.",
            0.91m,
            ["Slow-burn pacing", "Thoughtful narrative", "Focus-friendly atmosphere"]),
        new(
            "movie-about-time",
            RecommendationKind.Movie,
            MoodCategory.Nostalgic,
            "About Time",
            "Richard Curtis",
            "Romance / Drama",
            "TMDb",
            "A sentimental recommendation for reflective and emotionally warm moods.",
            0.93m,
            ["Reflective themes", "Emotional warmth", "High nostalgia fit"])
    ];

    public Task<IReadOnlyList<ContentRecommendation>> GetRecommendationsAsync(
        MoodSelection selection,
        CancellationToken cancellationToken = default)
    {
        var recommendations = Seeds
            .Where(seed => seed.Mood == selection.Mood)
            .Where(seed => selection.IncludeMusic || seed.Kind != RecommendationKind.Music)
            .Where(seed => selection.IncludeMovies || seed.Kind != RecommendationKind.Movie)
            .Where(seed => !selection.FamilyFriendlyOnly || seed.Kind != RecommendationKind.Movie || seed.IsFamilyFriendly)
            .Select(seed => seed.ToRecommendation(selection))
            .OrderByDescending(item => item.MatchScore)
            .ToArray();

        return Task.FromResult<IReadOnlyList<ContentRecommendation>>(recommendations);
    }

    private sealed record RecommendationSeed(
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
        public bool IsFamilyFriendly => Title is "Paddington 2";

        public ContentRecommendation ToRecommendation(MoodSelection selection)
        {
            var whyItMatches = Signals.ToList();
            var score = BaseScore;

            if (selection.EnergyLevel.HasValue)
            {
                whyItMatches.Add($"Energy filter: {selection.EnergyLevel.Value}");
            }

            if (selection.TimeOfDay.HasValue)
            {
                whyItMatches.Add($"Time of day: {selection.TimeOfDay}");
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
                    $"This {Kind.ToString().ToLowerInvariant()} aligns with a {selection.Mood.ToString().ToLowerInvariant()} mood.",
                    whyItMatches));
        }
    }
}
