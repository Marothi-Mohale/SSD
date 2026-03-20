using SSD.Application.Abstractions;
using SSD.Application.Contracts;
using SSD.Domain.Entities;
using SSD.Domain.ValueObjects;

namespace SSD.Application.Services;

public sealed class RecommendationService(
    IEnumerable<IRecommendationProvider> providers) : IRecommendationService
{
    private readonly IReadOnlyList<IRecommendationProvider> _providers = providers.ToList();

    public async Task<DiscoverRecommendationsResponse> DiscoverAsync(
        DiscoverRecommendationsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.IncludeMusic && !request.IncludeMovies)
        {
            throw new InvalidOperationException("At least one recommendation type must be selected.");
        }

        var selection = new MoodSelection(
            request.Mood,
            request.Energy,
            request.TimeOfDay,
            request.FamilyFriendlyOnly,
            request.IncludeMusic,
            request.IncludeMovies);

        var collected = new List<ContentRecommendation>();

        foreach (var provider in _providers)
        {
            var providerResults = await provider.GetRecommendationsAsync(selection, cancellationToken);
            collected.AddRange(providerResults);
        }

        var ordered = collected
            .OrderByDescending(item => item.MatchScore)
            .ThenBy(item => item.Title)
            .Take(8)
            .ToArray();

        return new DiscoverRecommendationsResponse(
            Guid.NewGuid().ToString("n"),
            ordered.Length == 0
                ? "No recommendations were found for the selected mood."
                : $"Found {ordered.Length} recommendations tailored to the selected mood.",
            ordered);
    }
}
