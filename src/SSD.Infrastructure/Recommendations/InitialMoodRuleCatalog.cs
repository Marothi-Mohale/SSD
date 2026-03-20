using SSD.Application.Abstractions;
using SSD.Domain.Enums;
using SSD.Domain.Moods;

namespace SSD.Infrastructure.Recommendations;

public sealed class InitialMoodRuleCatalog : IMoodRuleCatalog
{
    private static readonly MoodEngineConfiguration Configuration = BuildConfiguration();

    public MoodEngineConfiguration GetConfiguration() => Configuration;

    public MoodRuleDefinition GetRule(MoodCategory mood)
    {
        return Configuration.Rules.TryGetValue(mood, out var rule)
            ? rule
            : throw new KeyNotFoundException($"No mood rule has been configured for '{mood}'.");
    }

    private static MoodEngineConfiguration BuildConfiguration()
    {
        var rules = new[]
        {
            CreateHappyRule(),
            CreateSadRule(),
            CreateRomanticRule(),
            CreateAngryRule(),
            CreateFocusedRule(),
            CreateGymRule(),
            CreateRelaxedRule(),
            CreateNostalgicRule(),
            CreateLonelyRule(),
            CreatePartyRule(),
            CreateAdventurousRule(),
            CreateHeartbrokenRule(),
            CreateHopefulRule(),
            CreateRainyDayRule(),
            CreateLateNightRule()
        }.ToDictionary(rule => rule.Mood);

        return new MoodEngineConfiguration(MoodScoringWeights.Default, rules);
    }

    private static MoodRuleDefinition CreateHappyRule()
    {
        return new(
            MoodCategory.Happy,
            "Happy",
            "Optimistic, bright, easy-to-recommend content that lifts mood and keeps momentum positive.",
            Music: MediaRule(
                favoredAttributes: [Preference("uplifting", 1.0m), Preference("bright", 0.8m), Preference("groovy", 0.7m), Preference("warm", 0.6m)],
                favoredGenres: [Preference("pop", 1.0m), Preference("dance-pop", 0.8m), Preference("soul", 0.6m), Preference("funk", 0.7m)],
                fallbackGenres: ["indie-pop", "feel-good soundtrack"],
                excludedGenres: ["doom metal"],
                excludedAttributes: ["bleak", "brooding"]),
            Movie: MediaRule(
                favoredAttributes: [Preference("uplifting", 1.0m), Preference("feel-good", 0.9m), Preference("warm", 0.8m)],
                favoredGenres: [Preference("comedy", 1.0m), Preference("musical", 0.7m), Preference("family", 0.6m), Preference("coming-of-age", 0.6m)],
                fallbackGenres: ["adventure", "romantic comedy"],
                excludedGenres: ["horror", "tragedy"],
                excludedAttributes: ["grim", "nihilistic"]),
            ExclusionRules:
            [
                new MoodExclusionRule(RecommendationKind.Movie, MoodMatchField.Attribute, "nihilistic", 0.22m, "Too cynical for a happy mood.")
            ],
            TimeOfDayAdjustments:
            [
                MusicAndMovieAdjustment(TimeOfDaySegment.Morning, "Morning boost", ["bright"], ["uplifting"], ["feel-good"], ["warm"])
            ]);
    }

    private static MoodRuleDefinition CreateSadRule()
    {
        return new(
            MoodCategory.Sad,
            "Sad",
            "Gentle, empathetic recommendations that validate emotion without becoming emotionally numbing.",
            Music: MediaRule(
                favoredAttributes: [Preference("melancholic", 1.0m), Preference("reflective", 0.9m), Preference("tender", 0.7m)],
                favoredGenres: [Preference("indie-folk", 1.0m), Preference("singer-songwriter", 0.8m), Preference("ambient", 0.5m)],
                fallbackGenres: ["piano ballad", "alternative"],
                excludedGenres: ["party rap"],
                excludedAttributes: ["aggressive", "chaotic"]),
            Movie: MediaRule(
                favoredAttributes: [Preference("empathetic", 1.0m), Preference("reflective", 0.8m), Preference("quiet", 0.7m)],
                favoredGenres: [Preference("drama", 1.0m), Preference("indie drama", 0.8m), Preference("character study", 0.7m)],
                fallbackGenres: ["romance", "coming-of-age"],
                excludedGenres: ["slapstick comedy"],
                excludedAttributes: ["explosive", "frantic"]),
            ExclusionRules:
            [
                new MoodExclusionRule(RecommendationKind.Movie, MoodMatchField.Attribute, "hopeless", 0.20m, "Too emotionally flattening for a sad mood.")
            ],
            TimeOfDayAdjustments:
            [
                MusicAndMovieAdjustment(TimeOfDaySegment.Night, "Nighttime reflection boost", ["ambient"], ["reflective"], ["drama"], ["quiet"])
            ]);
    }

    private static MoodRuleDefinition CreateRomanticRule()
    {
        return new(
            MoodCategory.Romantic,
            "Romantic",
            "Intimate, warm, emotionally open content with chemistry, tenderness, and softness.",
            Music: MediaRule(
                favoredAttributes: [Preference("romantic", 1.0m), Preference("intimate", 0.8m), Preference("warm", 0.7m)],
                favoredGenres: [Preference("r-and-b", 1.0m), Preference("jazz", 0.7m), Preference("soul", 0.7m), Preference("acoustic pop", 0.6m)],
                fallbackGenres: ["soft rock", "latin pop"],
                excludedGenres: ["hardcore punk"],
                excludedAttributes: ["abrasive", "detached"]),
            Movie: MediaRule(
                favoredAttributes: [Preference("romantic", 1.0m), Preference("chemistry", 0.9m), Preference("tender", 0.7m)],
                favoredGenres: [Preference("romance", 1.0m), Preference("romantic drama", 0.8m), Preference("romantic comedy", 0.7m)],
                fallbackGenres: ["period drama", "drama"],
                excludedGenres: ["body horror"],
                excludedAttributes: ["cynical", "cold"]),
            ExclusionRules: [],
            TimeOfDayAdjustments:
            [
                MusicAndMovieAdjustment(TimeOfDaySegment.Evening, "Evening intimacy boost", ["intimate"], ["warm"], ["romance"], ["chemistry"])
            ]);
    }

    private static MoodRuleDefinition CreateAngryRule()
    {
        return new(
            MoodCategory.Angry,
            "Angry",
            "Cathartic, high-intensity content that channels frustration into momentum rather than chaos.",
            Music: MediaRule(
                favoredAttributes: [Preference("cathartic", 1.0m), Preference("aggressive", 0.8m), Preference("driving", 0.7m)],
                favoredGenres: [Preference("rock", 1.0m), Preference("metal", 0.8m), Preference("punk", 0.7m), Preference("industrial", 0.5m)],
                fallbackGenres: ["hip-hop", "hard rock"],
                excludedGenres: ["sleep music"],
                excludedAttributes: ["sleepy", "weightless"]),
            Movie: MediaRule(
                favoredAttributes: [Preference("revenge", 0.8m), Preference("adrenaline", 0.8m), Preference("defiant", 0.7m)],
                favoredGenres: [Preference("action", 1.0m), Preference("thriller", 0.8m), Preference("crime", 0.6m)],
                fallbackGenres: ["sports drama", "war"],
                excludedGenres: ["whimsical family"],
                excludedAttributes: ["saccharine", "meandering"]),
            ExclusionRules:
            [
                new MoodExclusionRule(RecommendationKind.Movie, MoodMatchField.Attribute, "nihilistic", 0.18m, "Anger should feel channelled, not empty.")
            ],
            TimeOfDayAdjustments:
            [
                MusicAndMovieAdjustment(TimeOfDaySegment.Afternoon, "Afternoon release boost", ["driving"], ["cathartic"], ["action"], ["adrenaline"])
            ]);
    }

    private static MoodRuleDefinition CreateFocusedRule()
    {
        return new(
            MoodCategory.Focused,
            "Focused",
            "Low-distraction, steady, cognitively supportive recommendations for work, study, or deep attention.",
            Music: MediaRule(
                favoredAttributes: [Preference("instrumental", 1.0m), Preference("steady", 0.9m), Preference("minimal", 0.8m), Preference("focus-friendly", 0.8m)],
                favoredGenres: [Preference("lo-fi", 1.0m), Preference("ambient", 0.8m), Preference("modern classical", 0.7m), Preference("downtempo", 0.6m)],
                fallbackGenres: ["post-rock", "jazz"],
                excludedGenres: ["party pop"],
                excludedAttributes: ["lyric-heavy", "chaotic"]),
            Movie: MediaRule(
                favoredAttributes: [Preference("thoughtful", 1.0m), Preference("immersive", 0.7m), Preference("measured", 0.7m)],
                favoredGenres: [Preference("sci-fi", 0.8m), Preference("drama", 0.6m), Preference("mystery", 0.7m)],
                fallbackGenres: ["documentary", "character study"],
                excludedGenres: ["gross-out comedy"],
                excludedAttributes: ["frantic", "noisy"]),
            ExclusionRules: [],
            TimeOfDayAdjustments:
            [
                MusicAndMovieAdjustment(TimeOfDaySegment.Morning, "Morning clarity boost", ["minimal"], ["steady"], ["documentary"], ["thoughtful"])
            ]);
    }

    private static MoodRuleDefinition CreateGymRule()
    {
        return new(
            MoodCategory.Gym,
            "Gym",
            "Motivating, kinetic, high-drive content built around momentum, confidence, and repetition.",
            Music: MediaRule(
                favoredAttributes: [Preference("motivational", 1.0m), Preference("driving", 0.9m), Preference("confident", 0.8m), Preference("high-energy", 0.8m)],
                favoredGenres: [Preference("hip-hop", 1.0m), Preference("edm", 0.8m), Preference("trap", 0.7m), Preference("rock", 0.6m)],
                fallbackGenres: ["drum and bass", "pop"],
                excludedGenres: ["sleep music"],
                excludedAttributes: ["fragile", "slow-burn"]),
            Movie: MediaRule(
                favoredAttributes: [Preference("motivational", 1.0m), Preference("competitive", 0.8m), Preference("underdog", 0.7m)],
                favoredGenres: [Preference("sports drama", 1.0m), Preference("action", 0.7m), Preference("biography", 0.5m)],
                fallbackGenres: ["adventure", "thriller"],
                excludedGenres: ["slow romance"],
                excludedAttributes: ["sedate", "bleak"]),
            ExclusionRules: [],
            TimeOfDayAdjustments:
            [
                MusicAndMovieAdjustment(TimeOfDaySegment.Morning, "Morning training boost", ["high-energy"], ["motivational"], ["sports drama"], ["underdog"])
            ]);
    }

    private static MoodRuleDefinition CreateRelaxedRule()
    {
        return new(
            MoodCategory.Relaxed,
            "Relaxed",
            "Soft, comforting, low-friction recommendations that reduce tension and invite ease.",
            Music: MediaRule(
                favoredAttributes: [Preference("calming", 1.0m), Preference("soft", 0.8m), Preference("spacious", 0.7m)],
                favoredGenres: [Preference("ambient", 1.0m), Preference("acoustic", 0.7m), Preference("chillout", 0.7m), Preference("bossa nova", 0.5m)],
                fallbackGenres: ["soft pop", "lo-fi"],
                excludedGenres: ["thrash metal"],
                excludedAttributes: ["abrasive", "jarring"]),
            Movie: MediaRule(
                favoredAttributes: [Preference("comforting", 1.0m), Preference("warm", 0.8m), Preference("gentle", 0.7m)],
                favoredGenres: [Preference("family", 0.8m), Preference("comedy", 0.7m), Preference("food film", 0.7m), Preference("slice-of-life", 0.6m)],
                fallbackGenres: ["romance", "travel"],
                excludedGenres: ["torture horror"],
                excludedAttributes: ["grim", "harsh"]),
            ExclusionRules: [],
            TimeOfDayAdjustments:
            [
                MusicAndMovieAdjustment(TimeOfDaySegment.Evening, "Evening wind-down boost", ["spacious"], ["soft"], ["slice-of-life"], ["gentle"])
            ]);
    }

    private static MoodRuleDefinition CreateNostalgicRule()
    {
        return new(
            MoodCategory.Nostalgic,
            "Nostalgic",
            "Recommendations that evoke memory, warmth, and a reflective sense of the past.",
            Music: MediaRule(
                favoredAttributes: [Preference("nostalgic", 1.0m), Preference("reflective", 0.8m), Preference("warm", 0.6m)],
                favoredGenres: [Preference("classic rock", 0.9m), Preference("retro pop", 0.8m), Preference("oldies", 0.8m), Preference("dream-pop", 0.5m)],
                fallbackGenres: ["folk", "soundtrack"],
                excludedGenres: ["abrasive edm"],
                excludedAttributes: ["hypermodern", "sterile"]),
            Movie: MediaRule(
                favoredAttributes: [Preference("reflective", 1.0m), Preference("bittersweet", 0.8m), Preference("warm", 0.7m)],
                favoredGenres: [Preference("drama", 0.8m), Preference("romance", 0.7m), Preference("coming-of-age", 0.7m)],
                fallbackGenres: ["period drama", "family"],
                excludedGenres: ["splatter horror"],
                excludedAttributes: ["cruel", "chaotic"]),
            ExclusionRules: [],
            TimeOfDayAdjustments:
            [
                MusicAndMovieAdjustment(TimeOfDaySegment.LateNight, "Late-night memory boost", ["nostalgic"], ["reflective"], ["period drama"], ["bittersweet"])
            ]);
    }

    private static MoodRuleDefinition CreateLonelyRule()
    {
        return new(
            MoodCategory.Lonely,
            "Lonely",
            "Companionable, intimate content that feels personal and emotionally present without overwhelming the listener or viewer.",
            Music: MediaRule(
                favoredAttributes: [Preference("intimate", 1.0m), Preference("reflective", 0.8m), Preference("gentle", 0.7m)],
                favoredGenres: [Preference("indie", 0.8m), Preference("singer-songwriter", 0.8m), Preference("dream-pop", 0.7m)],
                fallbackGenres: ["ambient", "acoustic"],
                excludedGenres: ["party anthem"],
                excludedAttributes: ["taunting", "chaotic"]),
            Movie: MediaRule(
                favoredAttributes: [Preference("empathetic", 1.0m), Preference("intimate", 0.8m), Preference("human", 0.7m)],
                favoredGenres: [Preference("drama", 1.0m), Preference("romance", 0.6m), Preference("science fiction", 0.5m)],
                fallbackGenres: ["indie drama", "character study"],
                excludedGenres: ["raucous comedy"],
                excludedAttributes: ["exploitative", "mean-spirited"]),
            ExclusionRules: [],
            TimeOfDayAdjustments:
            [
                MusicAndMovieAdjustment(TimeOfDaySegment.Night, "Night companionship boost", ["gentle"], ["intimate"], ["character study"], ["empathetic"])
            ]);
    }

    private static MoodRuleDefinition CreatePartyRule()
    {
        return new(
            MoodCategory.Party,
            "Party",
            "High-social-energy recommendations built for movement, brightness, rhythm, and easy momentum.",
            Music: MediaRule(
                favoredAttributes: [Preference("danceable", 1.0m), Preference("high-energy", 0.9m), Preference("social", 0.8m)],
                favoredGenres: [Preference("dance-pop", 1.0m), Preference("edm", 0.9m), Preference("hip-hop", 0.7m), Preference("house", 0.7m)],
                fallbackGenres: ["disco", "latin pop"],
                excludedGenres: ["funeral doom"],
                excludedAttributes: ["somber", "fragile"]),
            Movie: MediaRule(
                favoredAttributes: [Preference("chaotic-fun", 1.0m), Preference("social", 0.8m), Preference("fast", 0.7m)],
                favoredGenres: [Preference("comedy", 1.0m), Preference("teen comedy", 0.7m), Preference("music", 0.6m)],
                fallbackGenres: ["action comedy", "heist"],
                excludedGenres: ["slow drama"],
                excludedAttributes: ["meditative", "bleak"]),
            ExclusionRules: [],
            TimeOfDayAdjustments:
            [
                MusicAndMovieAdjustment(TimeOfDaySegment.Evening, "Evening party boost", ["danceable"], ["social"], ["comedy"], ["chaotic-fun"])
            ]);
    }

    private static MoodRuleDefinition CreateAdventurousRule()
    {
        return new(
            MoodCategory.Adventurous,
            "Adventurous",
            "Expansive, curiosity-forward recommendations that feel like movement, travel, risk, or discovery.",
            Music: MediaRule(
                favoredAttributes: [Preference("expansive", 1.0m), Preference("cinematic", 0.8m), Preference("uplifting", 0.6m)],
                favoredGenres: [Preference("indie rock", 0.8m), Preference("folk rock", 0.7m), Preference("soundtrack", 0.7m)],
                fallbackGenres: ["world", "alternative"],
                excludedGenres: ["sleep music"],
                excludedAttributes: ["claustrophobic", "stagnant"]),
            Movie: MediaRule(
                favoredAttributes: [Preference("exploratory", 1.0m), Preference("optimistic", 0.7m), Preference("wanderlust", 0.8m)],
                favoredGenres: [Preference("adventure", 1.0m), Preference("travel", 0.8m), Preference("fantasy", 0.6m)],
                fallbackGenres: ["action", "family"],
                excludedGenres: ["chamber drama"],
                excludedAttributes: ["stifling", "static"]),
            ExclusionRules: [],
            TimeOfDayAdjustments:
            [
                MusicAndMovieAdjustment(TimeOfDaySegment.Afternoon, "Daylight exploration boost", ["expansive"], ["uplifting"], ["adventure"], ["wanderlust"])
            ]);
    }

    private static MoodRuleDefinition CreateHeartbrokenRule()
    {
        return new(
            MoodCategory.Heartbroken,
            "Heartbroken",
            "Deeply emotional recommendations that validate loss and heartbreak while staying coherent and humane.",
            Music: MediaRule(
                favoredAttributes: [Preference("heartbreak", 1.0m), Preference("raw", 0.8m), Preference("vulnerable", 0.8m)],
                favoredGenres: [Preference("ballad", 1.0m), Preference("singer-songwriter", 0.8m), Preference("indie pop", 0.5m)],
                fallbackGenres: ["soul", "alternative"],
                excludedGenres: ["novelty dance"],
                excludedAttributes: ["flippant", "carefree"]),
            Movie: MediaRule(
                favoredAttributes: [Preference("bittersweet", 1.0m), Preference("romantic", 0.6m), Preference("devastating", 0.8m)],
                favoredGenres: [Preference("romantic drama", 1.0m), Preference("drama", 0.8m)],
                fallbackGenres: ["indie drama", "character study"],
                excludedGenres: ["broad comedy"],
                excludedAttributes: ["weightless", "giddy"]),
            ExclusionRules:
            [
                new MoodExclusionRule(RecommendationKind.Movie, MoodMatchField.Attribute, "cruel", 0.20m, "Too punishing for heartbroken users.")
            ],
            TimeOfDayAdjustments:
            [
                MusicAndMovieAdjustment(TimeOfDaySegment.LateNight, "Late-night heartbreak boost", ["raw"], ["vulnerable"], ["romantic drama"], ["bittersweet"])
            ]);
    }

    private static MoodRuleDefinition CreateHopefulRule()
    {
        return new(
            MoodCategory.Hopeful,
            "Hopeful",
            "Forward-looking recommendations that feel resilient, open, and emotionally restorative.",
            Music: MediaRule(
                favoredAttributes: [Preference("hopeful", 1.0m), Preference("uplifting", 0.8m), Preference("resilient", 0.7m)],
                favoredGenres: [Preference("pop", 0.8m), Preference("indie pop", 0.7m), Preference("gospel", 0.5m), Preference("soundtrack", 0.5m)],
                fallbackGenres: ["folk-pop", "soul"],
                excludedGenres: ["funeral doom"],
                excludedAttributes: ["defeatist", "nihilistic"]),
            Movie: MediaRule(
                favoredAttributes: [Preference("hopeful", 1.0m), Preference("resilient", 0.8m), Preference("inspiring", 0.8m)],
                favoredGenres: [Preference("drama", 0.8m), Preference("biography", 0.7m), Preference("family", 0.5m)],
                fallbackGenres: ["sports drama", "adventure"],
                excludedGenres: ["bleak thriller"],
                excludedAttributes: ["defeatist", "nihilistic"]),
            ExclusionRules: [],
            TimeOfDayAdjustments:
            [
                MusicAndMovieAdjustment(TimeOfDaySegment.Morning, "Morning optimism boost", ["hopeful"], ["uplifting"], ["family"], ["inspiring"])
            ]);
    }

    private static MoodRuleDefinition CreateRainyDayRule()
    {
        return new(
            MoodCategory.RainyDay,
            "Rainy Day",
            "Cozy, inward, weather-friendly recommendations for staying in and leaning into atmosphere.",
            Music: MediaRule(
                favoredAttributes: [Preference("atmospheric", 1.0m), Preference("cozy", 0.8m), Preference("soft", 0.7m)],
                favoredGenres: [Preference("indie folk", 0.8m), Preference("jazz", 0.7m), Preference("ambient", 0.7m)],
                fallbackGenres: ["acoustic", "lo-fi"],
                excludedGenres: ["festival edm"],
                excludedAttributes: ["blaring", "abrasive"]),
            Movie: MediaRule(
                favoredAttributes: [Preference("cozy", 1.0m), Preference("whimsical", 0.7m), Preference("atmospheric", 0.8m)],
                favoredGenres: [Preference("romance", 0.7m), Preference("comedy", 0.6m), Preference("drama", 0.6m)],
                fallbackGenres: ["slice-of-life", "family"],
                excludedGenres: ["extreme horror"],
                excludedAttributes: ["punishing", "bombastic"]),
            ExclusionRules: [],
            TimeOfDayAdjustments:
            [
                MusicAndMovieAdjustment(TimeOfDaySegment.Afternoon, "Rainy afternoon comfort boost", ["cozy"], ["soft"], ["slice-of-life"], ["whimsical"])
            ]);
    }

    private static MoodRuleDefinition CreateLateNightRule()
    {
        return new(
            MoodCategory.LateNight,
            "Late Night",
            "Moody, nocturnal recommendations for slower hours, reflection, or after-dark energy.",
            Music: MediaRule(
                favoredAttributes: [Preference("nocturnal", 1.0m), Preference("dreamy", 0.8m), Preference("moody", 0.8m), Preference("night-drive", 0.7m)],
                favoredGenres: [Preference("synth-pop", 0.8m), Preference("r-and-b", 0.7m), Preference("trip-hop", 0.7m), Preference("electronic", 0.6m)],
                fallbackGenres: ["ambient", "indie"],
                excludedGenres: ["children's music"],
                excludedAttributes: ["daytime", "cheery"]),
            Movie: MediaRule(
                favoredAttributes: [Preference("moody", 1.0m), Preference("stylish", 0.8m), Preference("immersive", 0.7m)],
                favoredGenres: [Preference("neo-noir", 0.9m), Preference("thriller", 0.8m), Preference("science fiction", 0.6m)],
                fallbackGenres: ["crime", "drama"],
                excludedGenres: ["broad family comedy"],
                excludedAttributes: ["cartoonish", "sunny"]),
            ExclusionRules: [],
            TimeOfDayAdjustments:
            [
                MusicAndMovieAdjustment(TimeOfDaySegment.LateNight, "After-hours atmosphere boost", ["night-drive"], ["nocturnal"], ["neo-noir"], ["stylish"])
            ]);
    }

    private static MoodMediaRule MediaRule(
        IReadOnlyList<WeightedPreference> favoredAttributes,
        IReadOnlyList<WeightedPreference> favoredGenres,
        IReadOnlyList<string> fallbackGenres,
        IReadOnlyList<string> excludedGenres,
        IReadOnlyList<string> excludedAttributes)
    {
        return new MoodMediaRule(favoredAttributes, favoredGenres, fallbackGenres, excludedGenres, excludedAttributes);
    }

    private static MoodTimeOfDayAdjustment MusicAndMovieAdjustment(
        TimeOfDaySegment timeOfDay,
        string note,
        IReadOnlyList<string> musicGenres,
        IReadOnlyList<string> musicAttributes,
        IReadOnlyList<string> movieGenres,
        IReadOnlyList<string> movieAttributes)
    {
        return new MoodTimeOfDayAdjustment(
            timeOfDay,
            note,
            musicGenres.Select(key => Preference(key, 1.0m)).ToArray(),
            musicAttributes.Select(key => Preference(key, 1.0m)).ToArray(),
            movieGenres.Select(key => Preference(key, 1.0m)).ToArray(),
            movieAttributes.Select(key => Preference(key, 1.0m)).ToArray());
    }

    private static WeightedPreference Preference(string key, decimal weight)
    {
        return new WeightedPreference(key, weight);
    }
}
