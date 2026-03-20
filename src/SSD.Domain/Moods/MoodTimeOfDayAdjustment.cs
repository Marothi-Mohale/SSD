using SSD.Domain.Enums;

namespace SSD.Domain.Moods;

public sealed record MoodTimeOfDayAdjustment(
    TimeOfDaySegment TimeOfDay,
    string Note,
    IReadOnlyList<WeightedPreference> MusicGenreAdjustments,
    IReadOnlyList<WeightedPreference> MusicAttributeAdjustments,
    IReadOnlyList<WeightedPreference> MovieGenreAdjustments,
    IReadOnlyList<WeightedPreference> MovieAttributeAdjustments);
