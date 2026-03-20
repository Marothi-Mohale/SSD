using SSD.Application.Abstractions;
using SSD.Application.Contracts;
using SSD.Application.Services;
using SSD.Domain.Entities;
using SSD.Domain.Enums;
using SSD.Domain.ValueObjects;

namespace SSD.Application.Tests;

public sealed class RecommendationServiceTests
{
    [Fact]
    public async Task DiscoverAsync_ReturnsRecommendationsOrderedByScore()
    {
        var service = new RecommendationService([new FakeProvider()]);

        var response = await service.DiscoverAsync(new DiscoverRecommendationsRequest(
            MoodCategory.Calm,
            "low",
            "evening",
            false,
            true,
            true));

        Assert.Equal("High Match", response.Recommendations[0].Title);
        Assert.Equal("Low Match", response.Recommendations[1].Title);
    }

    private sealed class FakeProvider : IRecommendationProvider
    {
        public Task<IReadOnlyList<ContentRecommendation>> GetRecommendationsAsync(
            MoodSelection selection,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ContentRecommendation> recommendations =
            [
                new ContentRecommendation(
                    "low",
                    RecommendationKind.Music,
                    "Low Match",
                    "Artist",
                    "Ambient",
                    "Test",
                    "Low score",
                    0.70m,
                    new RecommendationReason("Low", ["signal"])),
                new ContentRecommendation(
                    "high",
                    RecommendationKind.Movie,
                    "High Match",
                    "Director",
                    "Drama",
                    "Test",
                    "High score",
                    0.95m,
                    new RecommendationReason("High", ["signal"]))
            ];

            return Task.FromResult(recommendations);
        }
    }
}
