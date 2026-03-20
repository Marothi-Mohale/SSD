using SSD.Application.Abstractions;
using SSD.Application.Services;
using SSD.Domain.Enums;
using SSD.Domain.Moods;
using SSD.Domain.ValueObjects;

namespace SSD.Application.Tests;

public sealed class MoodRuleScorerTests
{
    private static readonly MoodRuleDefinition TestRule = new(
        MoodCategory.Relaxed,
        "Relaxed",
        "Test rule.",
        new MoodMediaRule(
            [new WeightedPreference("calming", 1.0m), new WeightedPreference("soft", 0.5m)],
            [new WeightedPreference("ambient", 1.0m), new WeightedPreference("acoustic", 0.5m)],
            ["lo-fi"],
            ["thrash metal"],
            ["abrasive"]),
        new MoodMediaRule(
            [new WeightedPreference("warm", 1.0m)],
            [new WeightedPreference("slice-of-life", 1.0m)],
            ["comedy"],
            ["torture horror"],
            ["grim"]),
        [],
        [
            new MoodTimeOfDayAdjustment(
                TimeOfDaySegment.Evening,
                "Evening wind-down boost",
                [new WeightedPreference("ambient", 1.0m)],
                [new WeightedPreference("soft", 1.0m)],
                [],
                [])
        ]);

    private readonly MoodRuleScorer _scorer = new(new StubMoodRuleCatalog(TestRule));

    [Fact]
    public void ScoreRewardsGenreAttributeAndTimeAlignment()
    {
        var selection = new MoodSelection(MoodCategory.Relaxed, EnergyLevel.Low, TimeOfDaySegment.Evening, false, true, true);
        var candidate = new RecommendationCandidateProfile(
            ["Ambient"],
            ["Calming", "Soft"],
            EnergyLevel.Low,
            TimeOfDaySegment.Evening,
            true);

        var result = _scorer.Score(selection, TestRule, RecommendationKind.Music, candidate);

        Assert.True(result.Score > 0.70m);
        Assert.Contains(result.Signals, signal => signal.Contains("Genre fit", StringComparison.Ordinal));
        Assert.Contains(result.Signals, signal => signal.Contains("Evening wind-down boost", StringComparison.Ordinal));
    }

    [Fact]
    public void ScoreFallsBackWhenPrimaryGenreMisses()
    {
        var selection = new MoodSelection(MoodCategory.Relaxed, EnergyLevel.Low, null, false, true, true);
        var candidate = new RecommendationCandidateProfile(
            ["Lo-Fi"],
            ["Soft"],
            EnergyLevel.Low,
            null,
            true);

        var result = _scorer.Score(selection, TestRule, RecommendationKind.Music, candidate);

        Assert.True(result.Score > 0.30m);
        Assert.Contains(result.Signals, signal => signal.Contains("Fallback genre", StringComparison.Ordinal));
    }

    [Fact]
    public void ScoreAppliesExclusionPenalty()
    {
        var selection = new MoodSelection(MoodCategory.Relaxed, EnergyLevel.High, null, false, true, true);
        var candidate = new RecommendationCandidateProfile(
            ["Thrash Metal"],
            ["Abrasive"],
            EnergyLevel.High,
            null,
            false);

        var result = _scorer.Score(selection, TestRule, RecommendationKind.Music, candidate);

        Assert.True(result.Score < 0.20m);
        Assert.Contains(result.Signals, signal => signal.Contains("Excluded", StringComparison.Ordinal));
    }

    private sealed class StubMoodRuleCatalog(MoodRuleDefinition rule) : IMoodRuleCatalog
    {
        private readonly MoodEngineConfiguration _configuration = new(
            MoodScoringWeights.Default,
            new Dictionary<MoodCategory, MoodRuleDefinition> { [rule.Mood] = rule });

        public MoodEngineConfiguration GetConfiguration() => _configuration;

        public MoodRuleDefinition GetRule(MoodCategory mood) => _configuration.Rules[mood];
    }
}
