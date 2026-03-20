using SSD.Mobile.Models;

namespace SSD.Mobile.Services;

public sealed class MockRecommendationApiClient
{
    public Task<IReadOnlyList<RecommendationCard>> DiscoverAsync(
        string mood,
        bool includeMusic,
        bool includeMovies,
        CancellationToken cancellationToken = default)
    {
        var items = new List<RecommendationCard>();

        if (includeMusic)
        {
            items.Add(new RecommendationCard(
                $"{mood} Mix",
                "Music recommendation",
                "A starter playlist placeholder for the selected mood.",
                $"Chosen because the user selected a {mood.ToLowerInvariant()} mood and asked for music."));
        }

        if (includeMovies)
        {
            items.Add(new RecommendationCard(
                $"{mood} Feature Pick",
                "Movie recommendation",
                "A starter movie placeholder for the selected mood.",
                $"Chosen because the user selected a {mood.ToLowerInvariant()} mood and asked for movies."));
        }

        return Task.FromResult<IReadOnlyList<RecommendationCard>>(items);
    }
}
