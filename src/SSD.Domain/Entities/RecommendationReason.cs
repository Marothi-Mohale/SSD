namespace SSD.Domain.Entities;

public sealed record RecommendationReason(
    string Summary,
    IReadOnlyList<string> Signals);
