using SSD.Domain.Common;
using SSD.Domain.Enums;
using SSD.Domain.ValueObjects;

namespace SSD.Domain.Entities;

public sealed class RecommendationItem : AuditableEntity
{
    public RecommendationItem()
    {
        Title = string.Empty;
        ProviderContentId = string.Empty;
        Explanation = new RecommendationExplanation("Recommendation explanation pending.");
        Genres = [];
    }

    public RecommendationItem(
        Guid id,
        Guid recommendationSessionId,
        RecommendationContentType contentType,
        RecommendationProviderType provider,
        string providerContentId,
        string title,
        RecommendationExplanation explanation)
        : base(id)
    {
        RecommendationSessionId = recommendationSessionId;
        ContentType = contentType;
        Provider = provider;
        ProviderContentId = providerContentId;
        Title = title;
        Explanation = explanation;
        Genres = [];
    }

    public Guid RecommendationSessionId { get; private set; }

    public RecommendationContentType ContentType { get; private set; }

    public RecommendationProviderType Provider { get; private set; }

    public string ProviderContentId { get; private set; }

    public string? ProviderContentUrl { get; private set; }

    public string Title { get; private set; }

    public string? SecondaryText { get; private set; }

    public string? Description { get; private set; }

    public List<string> Genres { get; private set; }

    public decimal MatchScore { get; private set; }

    public int Rank { get; private set; }

    public bool IsFamilyFriendly { get; private set; }

    public DateOnly? ReleaseDate { get; private set; }

    public string? ArtworkUrl { get; private set; }

    public string? PreviewUrl { get; private set; }

    public RecommendationExplanation Explanation { get; private set; }

    public RecommendationSession? RecommendationSession { get; private set; }
}
