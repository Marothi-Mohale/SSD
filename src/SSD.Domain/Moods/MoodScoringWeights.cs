namespace SSD.Domain.Moods;

public sealed record MoodScoringWeights(
    decimal BaseScore,
    decimal GenreWeight,
    decimal AttributeWeight,
    decimal FallbackGenreWeight,
    decimal RequestedTimeOfDayWeight,
    decimal TimeAdjustmentWeight,
    decimal EnergyWeight,
    decimal ExclusionPenaltyFloor)
{
    public static MoodScoringWeights Default { get; } = new(
        BaseScore: 0.20m,
        GenreWeight: 0.30m,
        AttributeWeight: 0.25m,
        FallbackGenreWeight: 0.10m,
        RequestedTimeOfDayWeight: 0.05m,
        TimeAdjustmentWeight: 0.05m,
        EnergyWeight: 0.05m,
        ExclusionPenaltyFloor: 0.15m);
}
