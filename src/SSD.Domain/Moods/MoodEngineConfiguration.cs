using SSD.Domain.Enums;

namespace SSD.Domain.Moods;

public sealed record MoodEngineConfiguration(
    MoodScoringWeights Weights,
    IReadOnlyDictionary<MoodCategory, MoodRuleDefinition> Rules);
