using SSD.Domain.Common;
using SSD.Domain.Enums;
using SSD.Domain.ValueObjects;

namespace SSD.Domain.Entities;

public sealed class RecommendationSession : AggregateRoot
{
    private readonly List<FeedbackEvent> _feedbackEvents = [];
    private readonly List<RecommendationItem> _items = [];

    public RecommendationSession()
    {
        CorrelationId = string.Empty;
        Selection = new MoodSelection(MoodCategory.Relaxed, null, null, false, true, true);
        Status = RecommendationSessionStatus.Pending;
    }

    public RecommendationSession(Guid id, Guid userId, string correlationId, MoodSelection selection)
        : base(id)
    {
        UserId = userId;
        CorrelationId = correlationId;
        Selection = selection;
        Status = RecommendationSessionStatus.Pending;
        RequestedUtc = CreatedUtc;
    }

    public Guid UserId { get; private set; }

    public string CorrelationId { get; private set; }

    public MoodSelection Selection { get; private set; }

    public RecommendationSessionStatus Status { get; private set; }

    public DateTimeOffset RequestedUtc { get; private set; }

    public DateTimeOffset? CompletedUtc { get; private set; }

    public int RecommendationCount { get; private set; }

    public string? FailureReason { get; private set; }

    public User? User { get; private set; }

    public IReadOnlyCollection<RecommendationItem> Items => _items;

    public IReadOnlyCollection<FeedbackEvent> FeedbackEvents => _feedbackEvents;
}
