using SSD.Domain.Common;
using SSD.Domain.Enums;

namespace SSD.Domain.Entities;

public sealed class SearchHistory : AuditableEntity
{
    public SearchHistory()
    {
        QueryText = string.Empty;
        FiltersJson = "{}";
        SearchDomain = SearchDomain.Mixed;
    }

    public SearchHistory(Guid id, Guid userId, SearchDomain searchDomain, string queryText)
        : base(id)
    {
        UserId = userId;
        SearchDomain = searchDomain;
        QueryText = queryText;
        FiltersJson = "{}";
    }

    public Guid UserId { get; private set; }

    public SearchDomain SearchDomain { get; private set; }

    public string QueryText { get; private set; }

    public string FiltersJson { get; private set; }

    public int ResultCount { get; private set; }

    public Guid? RecommendationSessionId { get; private set; }

    public User? User { get; private set; }

    public RecommendationSession? RecommendationSession { get; private set; }
}
