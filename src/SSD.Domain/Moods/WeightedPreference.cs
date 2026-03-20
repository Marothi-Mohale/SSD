namespace SSD.Domain.Moods;

public sealed record WeightedPreference(
    string Key,
    decimal Weight);
