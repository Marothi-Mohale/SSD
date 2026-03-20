using SSD.Domain.Enums;

namespace SSD.Domain.Moods;

public sealed record MoodExclusionRule(
    RecommendationKind Kind,
    MoodMatchField Field,
    string Value,
    decimal Penalty,
    string Reason);
