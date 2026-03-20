using SSD.Domain.Enums;

namespace SSD.Domain.Entities;

public sealed record ContentRecommendation(
    string Id,
    RecommendationKind Kind,
    string Title,
    string Creator,
    string Genre,
    string Provider,
    string Description,
    decimal MatchScore,
    RecommendationReason WhyItMatches);
