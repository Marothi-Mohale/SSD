using SSD.Domain.Entities;
using SSD.Domain.ValueObjects;

namespace SSD.Application.Abstractions;

public interface IRecommendationProvider
{
    Task<IReadOnlyList<ContentRecommendation>> GetRecommendationsAsync(
        MoodSelection selection,
        CancellationToken cancellationToken = default);
}
