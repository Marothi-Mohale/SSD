using SSD.Domain.Common;
using SSD.Domain.Enums;

namespace SSD.Domain.Entities;

public sealed class FeedbackEvent : AuditableEntity
{
    public FeedbackEvent()
    {
        FeedbackType = FeedbackType.Skip;
    }

    public FeedbackEvent(Guid id, Guid recommendationSessionId, Guid userId, FeedbackType feedbackType)
        : base(id)
    {
        RecommendationSessionId = recommendationSessionId;
        UserId = userId;
        FeedbackType = feedbackType;
    }

    public Guid RecommendationSessionId { get; private set; }

    public Guid UserId { get; private set; }

    public Guid? RecommendationItemId { get; private set; }

    public FeedbackType FeedbackType { get; private set; }

    public string? Reason { get; private set; }

    public RecommendationSession? RecommendationSession { get; private set; }

    public RecommendationItem? RecommendationItem { get; private set; }

    public User? User { get; private set; }
}
