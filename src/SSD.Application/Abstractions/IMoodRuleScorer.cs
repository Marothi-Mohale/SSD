using SSD.Application.Models;
using SSD.Domain.Enums;
using SSD.Domain.Moods;
using SSD.Domain.ValueObjects;

namespace SSD.Application.Abstractions;

public interface IMoodRuleScorer
{
    MoodScoreResult Score(
        MoodSelection selection,
        MoodRuleDefinition rule,
        RecommendationKind kind,
        RecommendationCandidateProfile candidate);
}
