namespace SSD.Domain.Moods;

public sealed record MoodMediaRule(
    IReadOnlyList<WeightedPreference> FavoredAttributes,
    IReadOnlyList<WeightedPreference> FavoredGenres,
    IReadOnlyList<string> FallbackGenres,
    IReadOnlyList<string> ExcludedGenres,
    IReadOnlyList<string> ExcludedAttributes);
