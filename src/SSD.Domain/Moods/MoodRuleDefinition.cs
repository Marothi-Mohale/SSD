using SSD.Domain.Enums;

namespace SSD.Domain.Moods;

public sealed record MoodRuleDefinition(
    MoodCategory Mood,
    string DisplayName,
    string Description,
    MoodMediaRule Music,
    MoodMediaRule Movie,
    IReadOnlyList<MoodExclusionRule> ExclusionRules,
    IReadOnlyList<MoodTimeOfDayAdjustment> TimeOfDayAdjustments);
