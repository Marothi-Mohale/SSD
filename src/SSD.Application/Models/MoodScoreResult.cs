namespace SSD.Application.Models;

public sealed record MoodScoreResult(
    decimal Score,
    string Summary,
    IReadOnlyList<string> Signals);
