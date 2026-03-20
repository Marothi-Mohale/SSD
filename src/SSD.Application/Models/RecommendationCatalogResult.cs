using SSD.Domain.Entities;

namespace SSD.Application.Models;

public sealed record RecommendationCatalogResult(
    IReadOnlyList<ContentRecommendation> Recommendations);
