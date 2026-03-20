using SSD.Domain.Enums;

namespace SSD.Domain.ValueObjects;

public sealed record MoodSelection(
    MoodCategory Mood,
    EnergyLevel? EnergyLevel,
    TimeOfDaySegment? TimeOfDay,
    bool FamilyFriendlyOnly,
    bool IncludeMusic,
    bool IncludeMovies);
