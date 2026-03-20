# SSD Mood Engine v1

## Goal

SSD v1 uses a deterministic mood engine instead of ML. The engine maps a selected mood to explicit rule definitions, then scores recommendation candidates against those rules using genres, tonal attributes, energy, family-safety, and time-of-day adjustments.

This keeps v1:

- deterministic
- easy to test
- easy to tune by editing configuration data
- compatible with future hybrid AI ranking

## Taxonomy

The v1 mood taxonomy is:

- happy
- sad
- romantic
- angry
- focused
- gym
- relaxed
- nostalgic
- lonely
- party
- adventurous
- heartbroken
- hopeful
- rainy day
- late night

Each mood definition includes:

- a description
- favored music attributes
- favored movie attributes
- favored genres
- fallback genres
- explicit exclusion rules
- optional time-of-day boosts

## Domain Model

Core mood-engine models live in `SSD.Domain/Moods`:

- `MoodRuleDefinition`
- `MoodMediaRule`
- `MoodExclusionRule`
- `MoodTimeOfDayAdjustment`
- `WeightedPreference`
- `RecommendationCandidateProfile`
- `MoodScoringWeights`
- `MoodEngineConfiguration`

Application services:

- `IMoodRuleCatalog`
- `IMoodRuleScorer`
- `MoodRuleScorer`

Infrastructure data:

- `InitialMoodRuleCatalog`

## Scoring Algorithm

Score each candidate with these weighted components:

- base score: `0.20`
- primary genre fit: up to `0.30`
- attribute fit: up to `0.25`
- fallback genre fit: up to `0.10`
- direct requested time-of-day fit: up to `0.05`
- time-of-day rule adjustment: up to `0.05`
- energy alignment: up to `0.05`
- exclusion penalties: subtract at least `0.15` per hit

Final score is clamped to `0.01` through `0.99`.

## Weight Rationale

- Genre gets the largest weight because it is the fastest stable proxy for emotional fit in a metadata-first system.
- Attributes get the next largest weight because they capture pacing and tone that genre alone misses.
- Fallback genres prevent sparse catalogs from returning nothing for narrower moods.
- Time-of-day boosts are intentionally small nudges rather than dominant ranking factors.
- Energy is a tie-breaker and should not overpower mood itself.
- Exclusions are strong enough to push clearly wrong recommendations out of the top results.

## Why This Migrates Well To Hybrid AI

The v1 engine separates:

- mood rules
- candidate metadata
- scoring

That means future AI or personalization layers can:

- generate better candidate attributes
- rerank deterministic candidates
- learn user-specific weight overrides
- explain results using the same rule signals already produced in v1

The deterministic score can remain a transparent baseline even after AI-assisted ranking is introduced.
