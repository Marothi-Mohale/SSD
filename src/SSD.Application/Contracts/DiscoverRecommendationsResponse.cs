using SSD.Domain.Entities;

namespace SSD.Application.Contracts;

public sealed record DiscoverRecommendationsResponse(
    string CorrelationId,
    string Message,
    IReadOnlyList<ContentRecommendation> Recommendations);
