using SSD.Domain.Common;
using SSD.Domain.Enums;

namespace SSD.Domain.Entities;

public sealed class FavoriteItem : AuditableEntity
{
    public FavoriteItem()
    {
        ProviderContentId = string.Empty;
        Title = string.Empty;
    }

    public FavoriteItem(
        Guid id,
        Guid userId,
        RecommendationContentType contentType,
        RecommendationProviderType provider,
        string providerContentId,
        string title)
        : base(id)
    {
        UserId = userId;
        ContentType = contentType;
        Provider = provider;
        ProviderContentId = providerContentId;
        Title = title;
    }

    public Guid UserId { get; private set; }

    public RecommendationContentType ContentType { get; private set; }

    public RecommendationProviderType Provider { get; private set; }

    public string ProviderContentId { get; private set; }

    public string? ProviderContentUrl { get; private set; }

    public string Title { get; private set; }

    public string? SecondaryText { get; private set; }

    public string? ArtworkUrl { get; private set; }

    public Guid? SourceRecommendationItemId { get; private set; }

    public User? User { get; private set; }
}
