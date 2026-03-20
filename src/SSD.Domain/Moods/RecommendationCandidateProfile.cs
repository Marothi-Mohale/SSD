using SSD.Domain.Enums;

namespace SSD.Domain.Moods;

public sealed record RecommendationCandidateProfile(
    IReadOnlyList<string> Genres,
    IReadOnlyList<string> Attributes,
    EnergyLevel? EnergyLevel,
    TimeOfDaySegment? BestForTimeOfDay,
    bool IsFamilyFriendly);
