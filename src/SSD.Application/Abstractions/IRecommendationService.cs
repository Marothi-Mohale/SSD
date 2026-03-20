using SSD.Application.Contracts;

namespace SSD.Application.Abstractions;

public interface IRecommendationService
{
    Task<DiscoverRecommendationsResponse> DiscoverAsync(
        DiscoverRecommendationsRequest request,
        CancellationToken cancellationToken = default);
}
