using SSD.Domain.Entities;
using SSD.Mobile.Models;

namespace SSD.Mobile.Services;

public static class RecommendationFormatter
{
    public static RecommendationCardViewModel ToCard(ContentRecommendation recommendation)
    {
        return new RecommendationCardViewModel(
            recommendation.Title,
            $"{recommendation.Kind} · {recommendation.Creator}",
            recommendation.Description,
            recommendation.WhyItMatches.Summary);
    }
}
