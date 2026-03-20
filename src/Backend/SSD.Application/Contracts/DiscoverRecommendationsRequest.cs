using SSD.Domain.Enums;

namespace SSD.Application.Contracts;

public sealed record DiscoverRecommendationsRequest(
    MoodCategory Mood,
    string? Energy,
    string? TimeOfDay,
    bool FamilyFriendlyOnly,
    bool IncludeMusic = true,
    bool IncludeMovies = true);
