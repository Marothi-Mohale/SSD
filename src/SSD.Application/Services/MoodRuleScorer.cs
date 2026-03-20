using SSD.Application.Abstractions;
using SSD.Application.Models;
using SSD.Domain.Enums;
using SSD.Domain.Moods;
using SSD.Domain.ValueObjects;

namespace SSD.Application.Services;

public sealed class MoodRuleScorer(IMoodRuleCatalog catalog) : IMoodRuleScorer
{
    private readonly MoodScoringWeights _weights = catalog.GetConfiguration().Weights;

    public MoodScoreResult Score(
        MoodSelection selection,
        MoodRuleDefinition rule,
        RecommendationKind kind,
        RecommendationCandidateProfile candidate)
    {
        var mediaRule = kind == RecommendationKind.Music ? rule.Music : rule.Movie;
        var normalizedGenres = candidate.Genres.Select(Normalize).Where(value => value.Length > 0).ToHashSet(StringComparer.Ordinal);
        var normalizedAttributes = candidate.Attributes.Select(Normalize).Where(value => value.Length > 0).ToHashSet(StringComparer.Ordinal);

        var score = _weights.BaseScore;
        var signals = new List<string> { $"Mood: {rule.DisplayName}" };

        var matchedGenreWeights = mediaRule.FavoredGenres
            .Where(preference => normalizedGenres.Contains(Normalize(preference.Key)))
            .ToArray();

        var totalGenreWeight = mediaRule.FavoredGenres.Sum(preference => preference.Weight);
        if (matchedGenreWeights.Length > 0 && totalGenreWeight > 0m)
        {
            var genreContribution = _weights.GenreWeight * (matchedGenreWeights.Sum(preference => preference.Weight) / totalGenreWeight);
            score += genreContribution;
            signals.Add($"Genre fit: {string.Join(", ", matchedGenreWeights.Select(preference => preference.Key))}");
        }
        else if (mediaRule.FallbackGenres.Any(genre => normalizedGenres.Contains(Normalize(genre))))
        {
            score += _weights.FallbackGenreWeight;
            signals.Add("Fallback genre match");
        }

        var matchedAttributeWeights = mediaRule.FavoredAttributes
            .Where(preference => normalizedAttributes.Contains(Normalize(preference.Key)))
            .ToArray();

        var totalAttributeWeight = mediaRule.FavoredAttributes.Sum(preference => preference.Weight);
        if (matchedAttributeWeights.Length > 0 && totalAttributeWeight > 0m)
        {
            var attributeContribution = _weights.AttributeWeight * (matchedAttributeWeights.Sum(preference => preference.Weight) / totalAttributeWeight);
            score += attributeContribution;
            signals.Add($"Attribute fit: {string.Join(", ", matchedAttributeWeights.Select(preference => preference.Key))}");
        }

        if (selection.TimeOfDay.HasValue && candidate.BestForTimeOfDay == selection.TimeOfDay)
        {
            score += _weights.RequestedTimeOfDayWeight;
            signals.Add($"Time fit: {selection.TimeOfDay.Value}");
        }

        if (selection.TimeOfDay.HasValue)
        {
            var adjustment = rule.TimeOfDayAdjustments.FirstOrDefault(candidateAdjustment => candidateAdjustment.TimeOfDay == selection.TimeOfDay.Value);
            if (adjustment is not null)
            {
                var adjustmentContribution = CalculateAdjustmentContribution(kind, adjustment, normalizedGenres, normalizedAttributes);
                if (adjustmentContribution > 0m)
                {
                    score += adjustmentContribution;
                    signals.Add(adjustment.Note);
                }
            }
        }

        if (selection.EnergyLevel.HasValue && candidate.EnergyLevel.HasValue)
        {
            var energyDifference = Math.Abs((int)selection.EnergyLevel.Value - (int)candidate.EnergyLevel.Value);
            if (energyDifference == 0)
            {
                score += _weights.EnergyWeight;
                signals.Add($"Energy aligned: {selection.EnergyLevel.Value}");
            }
            else if (energyDifference == 1)
            {
                score += _weights.EnergyWeight / 2m;
                signals.Add("Energy adjacent fit");
            }
        }

        if (selection.FamilyFriendlyOnly && !candidate.IsFamilyFriendly)
        {
            score = Math.Max(0.01m, score - 0.35m);
            signals.Add("Family filter penalty");
        }

        var penaltyReasons = GetPenaltyReasons(rule.ExclusionRules, mediaRule, kind, normalizedGenres, normalizedAttributes);
        foreach (var penaltyReason in penaltyReasons)
        {
            score -= penaltyReason.penalty;
            signals.Add(penaltyReason.reason);
        }

        score = decimal.Clamp(score, 0.01m, 0.99m);

        var summary = matchedGenreWeights.Length > 0 || matchedAttributeWeights.Length > 0
            ? $"Matches the {rule.DisplayName.ToLowerInvariant()} mood through genre and tone alignment."
            : $"Uses fallback mood rules for a {rule.DisplayName.ToLowerInvariant()} recommendation.";

        return new MoodScoreResult(
            score,
            summary,
            signals.Take(5).ToArray());
    }

    private decimal CalculateAdjustmentContribution(
        RecommendationKind kind,
        MoodTimeOfDayAdjustment adjustment,
        HashSet<string> normalizedGenres,
        HashSet<string> normalizedAttributes)
    {
        var genreAdjustments = kind == RecommendationKind.Music
            ? adjustment.MusicGenreAdjustments
            : adjustment.MovieGenreAdjustments;
        var attributeAdjustments = kind == RecommendationKind.Music
            ? adjustment.MusicAttributeAdjustments
            : adjustment.MovieAttributeAdjustments;

        var matchedWeight = genreAdjustments
            .Where(preference => normalizedGenres.Contains(Normalize(preference.Key)))
            .Sum(preference => preference.Weight);

        matchedWeight += attributeAdjustments
            .Where(preference => normalizedAttributes.Contains(Normalize(preference.Key)))
            .Sum(preference => preference.Weight);

        var totalWeight = genreAdjustments.Sum(preference => preference.Weight) + attributeAdjustments.Sum(preference => preference.Weight);
        if (matchedWeight <= 0m || totalWeight <= 0m)
        {
            return 0m;
        }

        return _weights.TimeAdjustmentWeight * (matchedWeight / totalWeight);
    }

    private IEnumerable<(decimal penalty, string reason)> GetPenaltyReasons(
        IReadOnlyList<MoodExclusionRule> exclusions,
        MoodMediaRule mediaRule,
        RecommendationKind kind,
        HashSet<string> normalizedGenres,
        HashSet<string> normalizedAttributes)
    {
        foreach (var excludedGenre in mediaRule.ExcludedGenres.Where(excludedGenre => normalizedGenres.Contains(Normalize(excludedGenre))))
        {
            yield return (_weights.ExclusionPenaltyFloor, $"Excluded genre: {excludedGenre}");
        }

        foreach (var excludedAttribute in mediaRule.ExcludedAttributes.Where(excludedAttribute => normalizedAttributes.Contains(Normalize(excludedAttribute))))
        {
            yield return (_weights.ExclusionPenaltyFloor, $"Excluded attribute: {excludedAttribute}");
        }

        foreach (var exclusion in exclusions.Where(exclusion => exclusion.Kind == kind))
        {
            var matched = exclusion.Field switch
            {
                MoodMatchField.Genre => normalizedGenres.Contains(Normalize(exclusion.Value)),
                MoodMatchField.Attribute => normalizedAttributes.Contains(Normalize(exclusion.Value)),
                _ => false
            };

            if (matched)
            {
                yield return (Math.Max(exclusion.Penalty, _weights.ExclusionPenaltyFloor), exclusion.Reason);
            }
        }
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToLowerInvariant();
    }
}
