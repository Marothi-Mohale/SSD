using SSD.Domain.Enums;

namespace SSD.Domain.ValueObjects;

public sealed record MoodSelection(
    MoodCategory Mood,
    string? Energy,
    string? TimeOfDay,
    bool FamilyFriendlyOnly,
    bool IncludeMusic,
    bool IncludeMovies);
