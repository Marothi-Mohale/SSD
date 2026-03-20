using SSD.Application.Abstractions;
using SSD.Application.Models;
using SSD.Domain.Entities;
using SSD.Domain.Enums;
using SSD.Domain.Moods;
using SSD.Domain.ValueObjects;

namespace SSD.Infrastructure.Recommendations;

public sealed class SeedRecommendationProvider(
    IMoodRuleCatalog moodRuleCatalog,
    IMoodRuleScorer moodRuleScorer) : IRecommendationProvider
{
    private const decimal MinimumDisplayScore = 0.32m;

    private static readonly IReadOnlyList<RecommendationSeed> Seeds =
    [
        new(
            "music-happy-happy",
            RecommendationKind.Music,
            "Happy",
            "Pharrell Williams",
            "Spotify",
            "Bright pop with an easy groove and a celebratory hook.",
            new RecommendationCandidateProfile(["Pop", "Dance-Pop"], ["uplifting", "bright", "groovy", "warm"], EnergyLevel.High, TimeOfDaySegment.Morning, true)),
        new(
            "movie-happy-sing-street",
            RecommendationKind.Movie,
            "Sing Street",
            "John Carney",
            "TMDb",
            "A warm, funny coming-of-age story with music, optimism, and emotional lift.",
            new RecommendationCandidateProfile(["Comedy", "Coming-of-Age", "Music"], ["uplifting", "feel-good", "warm"], EnergyLevel.Medium, TimeOfDaySegment.Evening, true)),
        new(
            "music-sad-skinny-love",
            RecommendationKind.Music,
            "Skinny Love",
            "Bon Iver",
            "Spotify",
            "Tender indie-folk that validates sadness without becoming melodramatic.",
            new RecommendationCandidateProfile(["Indie-Folk", "Singer-Songwriter"], ["melancholic", "reflective", "tender"], EnergyLevel.Low, TimeOfDaySegment.Night, false)),
        new(
            "movie-sad-manchester-by-the-sea",
            RecommendationKind.Movie,
            "Manchester by the Sea",
            "Kenneth Lonergan",
            "TMDb",
            "A reflective drama grounded in grief, empathy, and quiet emotional realism.",
            new RecommendationCandidateProfile(["Drama", "Character Study"], ["empathetic", "reflective", "quiet"], EnergyLevel.Low, TimeOfDaySegment.Night, false)),
        new(
            "music-romantic-at-last",
            RecommendationKind.Music,
            "At Last",
            "Etta James",
            "Spotify",
            "Soulful and intimate, with timeless romantic warmth.",
            new RecommendationCandidateProfile(["Soul", "Jazz"], ["romantic", "intimate", "warm"], EnergyLevel.Low, TimeOfDaySegment.Evening, false)),
        new(
            "movie-romantic-before-sunrise",
            RecommendationKind.Movie,
            "Before Sunrise",
            "Richard Linklater",
            "TMDb",
            "Chemistry-driven romance built on conversation, intimacy, and tenderness.",
            new RecommendationCandidateProfile(["Romance", "Drama"], ["romantic", "chemistry", "tender"], EnergyLevel.Low, TimeOfDaySegment.Evening, false)),
        new(
            "music-angry-killing-in-the-name",
            RecommendationKind.Music,
            "Killing in the Name",
            "Rage Against the Machine",
            "Spotify",
            "Aggressive and cathartic with direct release-valve energy.",
            new RecommendationCandidateProfile(["Rock", "Metal"], ["cathartic", "aggressive", "driving"], EnergyLevel.High, TimeOfDaySegment.Afternoon, false)),
        new(
            "movie-angry-mad-max-fury-road",
            RecommendationKind.Movie,
            "Mad Max: Fury Road",
            "George Miller",
            "TMDb",
            "A propulsive action film that channels anger into adrenaline and motion.",
            new RecommendationCandidateProfile(["Action", "Thriller"], ["adrenaline", "defiant", "fast"], EnergyLevel.High, TimeOfDaySegment.Afternoon, false)),
        new(
            "music-focused-lofi-session",
            RecommendationKind.Music,
            "Lofi Study Session",
            "Various Artists",
            "Spotify",
            "Steady instrumental beats designed to help attention settle.",
            new RecommendationCandidateProfile(["Lo-fi", "Ambient"], ["instrumental", "steady", "minimal", "focus-friendly"], EnergyLevel.Low, TimeOfDaySegment.Morning, true)),
        new(
            "movie-focused-arrival",
            RecommendationKind.Movie,
            "Arrival",
            "Denis Villeneuve",
            "TMDb",
            "A measured, immersive science-fiction drama that rewards concentration.",
            new RecommendationCandidateProfile(["Sci-Fi", "Drama", "Mystery"], ["thoughtful", "immersive", "measured"], EnergyLevel.Low, TimeOfDaySegment.Morning, false)),
        new(
            "music-gym-lose-yourself",
            RecommendationKind.Music,
            "Lose Yourself",
            "Eminem",
            "Spotify",
            "High-drive motivation with confident, locked-in momentum.",
            new RecommendationCandidateProfile(["Hip-Hop"], ["motivational", "driving", "confident", "high-energy"], EnergyLevel.High, TimeOfDaySegment.Morning, false)),
        new(
            "movie-gym-creed",
            RecommendationKind.Movie,
            "Creed",
            "Ryan Coogler",
            "TMDb",
            "Underdog sports drama with strong training and perseverance energy.",
            new RecommendationCandidateProfile(["Sports Drama", "Drama"], ["motivational", "competitive", "underdog"], EnergyLevel.High, TimeOfDaySegment.Morning, false)),
        new(
            "music-relaxed-weightless",
            RecommendationKind.Music,
            "Weightless",
            "Marconi Union",
            "Spotify",
            "Gentle ambient textures for slowing down and decompressing.",
            new RecommendationCandidateProfile(["Ambient", "Chillout"], ["calming", "soft", "spacious"], EnergyLevel.Low, TimeOfDaySegment.Evening, true)),
        new(
            "movie-relaxed-chef",
            RecommendationKind.Movie,
            "Chef",
            "Jon Favreau",
            "TMDb",
            "Comforting, warm, and low-friction with easy feel-good momentum.",
            new RecommendationCandidateProfile(["Comedy", "Food Film", "Slice-of-Life"], ["comforting", "warm", "gentle"], EnergyLevel.Low, TimeOfDaySegment.Evening, true)),
        new(
            "music-nostalgic-dreams",
            RecommendationKind.Music,
            "Dreams",
            "Fleetwood Mac",
            "Spotify",
            "A reflective classic with warmth, memory, and easy emotional pull.",
            new RecommendationCandidateProfile(["Classic Rock", "Retro Pop"], ["nostalgic", "reflective", "warm"], EnergyLevel.Medium, TimeOfDaySegment.LateNight, false)),
        new(
            "movie-nostalgic-about-time",
            RecommendationKind.Movie,
            "About Time",
            "Richard Curtis",
            "TMDb",
            "Bittersweet, warm, and memory-rich without losing hope.",
            new RecommendationCandidateProfile(["Romance", "Drama", "Coming-of-Age"], ["reflective", "bittersweet", "warm"], EnergyLevel.Low, TimeOfDaySegment.LateNight, false)),
        new(
            "music-lonely-liability",
            RecommendationKind.Music,
            "Liability",
            "Lorde",
            "Spotify",
            "Sparse and intimate, like a quiet conversation with one person.",
            new RecommendationCandidateProfile(["Indie", "Singer-Songwriter"], ["intimate", "reflective", "gentle"], EnergyLevel.Low, TimeOfDaySegment.Night, false)),
        new(
            "movie-lonely-her",
            RecommendationKind.Movie,
            "Her",
            "Spike Jonze",
            "TMDb",
            "An intimate, empathetic character study about connection and isolation.",
            new RecommendationCandidateProfile(["Drama", "Science Fiction", "Character Study"], ["empathetic", "intimate", "human"], EnergyLevel.Low, TimeOfDaySegment.Night, false)),
        new(
            "music-party-uptown-funk",
            RecommendationKind.Music,
            "Uptown Funk",
            "Mark Ronson feat. Bruno Mars",
            "Spotify",
            "Immediate social energy with strong dance-floor momentum.",
            new RecommendationCandidateProfile(["Dance-Pop", "Funk"], ["danceable", "high-energy", "social"], EnergyLevel.High, TimeOfDaySegment.Evening, true)),
        new(
            "movie-party-booksmart",
            RecommendationKind.Movie,
            "Booksmart",
            "Olivia Wilde",
            "TMDb",
            "Fast, social, chaotic-fun comedy energy for party mode.",
            new RecommendationCandidateProfile(["Comedy", "Teen Comedy"], ["chaotic-fun", "social", "fast"], EnergyLevel.High, TimeOfDaySegment.Evening, false)),
        new(
            "music-adventurous-on-top-of-the-world",
            RecommendationKind.Music,
            "On Top of the World",
            "Imagine Dragons",
            "Spotify",
            "Expansive and forward-moving with travel-friendly optimism.",
            new RecommendationCandidateProfile(["Indie Rock", "Folk Rock"], ["expansive", "cinematic", "uplifting"], EnergyLevel.Medium, TimeOfDaySegment.Afternoon, true)),
        new(
            "movie-adventurous-walter-mitty",
            RecommendationKind.Movie,
            "The Secret Life of Walter Mitty",
            "Ben Stiller",
            "TMDb",
            "A hopeful travel-forward movie for curiosity, movement, and discovery.",
            new RecommendationCandidateProfile(["Adventure", "Travel", "Comedy"], ["exploratory", "optimistic", "wanderlust"], EnergyLevel.Medium, TimeOfDaySegment.Afternoon, true)),
        new(
            "music-heartbroken-someone-like-you",
            RecommendationKind.Music,
            "Someone Like You",
            "Adele",
            "Spotify",
            "Raw heartbreak with clarity, vulnerability, and emotional release.",
            new RecommendationCandidateProfile(["Ballad", "Soul"], ["heartbreak", "raw", "vulnerable"], EnergyLevel.Low, TimeOfDaySegment.LateNight, false)),
        new(
            "movie-heartbroken-blue-valentine",
            RecommendationKind.Movie,
            "Blue Valentine",
            "Derek Cianfrance",
            "TMDb",
            "A devastating romantic drama for heartbreak and emotional processing.",
            new RecommendationCandidateProfile(["Romantic Drama", "Drama"], ["bittersweet", "devastating", "romantic"], EnergyLevel.Low, TimeOfDaySegment.LateNight, false)),
        new(
            "music-hopeful-unwritten",
            RecommendationKind.Music,
            "Unwritten",
            "Natasha Bedingfield",
            "Spotify",
            "Open-hearted and resilient with clear upward emotional motion.",
            new RecommendationCandidateProfile(["Pop", "Indie Pop"], ["hopeful", "uplifting", "resilient"], EnergyLevel.Medium, TimeOfDaySegment.Morning, true)),
        new(
            "movie-hopeful-pursuit-of-happyness",
            RecommendationKind.Movie,
            "The Pursuit of Happyness",
            "Gabriele Muccino",
            "TMDb",
            "A resilient, inspiring drama rooted in effort and optimism.",
            new RecommendationCandidateProfile(["Drama", "Biography"], ["hopeful", "resilient", "inspiring"], EnergyLevel.Medium, TimeOfDaySegment.Morning, false)),
        new(
            "music-rainyday-holocene",
            RecommendationKind.Music,
            "Holocene",
            "Bon Iver",
            "Spotify",
            "Soft, atmospheric, and ideal for a slow rainy afternoon.",
            new RecommendationCandidateProfile(["Indie Folk", "Acoustic"], ["atmospheric", "cozy", "soft"], EnergyLevel.Low, TimeOfDaySegment.Afternoon, false)),
        new(
            "movie-rainyday-amelie",
            RecommendationKind.Movie,
            "Amelie",
            "Jean-Pierre Jeunet",
            "TMDb",
            "Whimsical and atmospheric with cozy inside-day energy.",
            new RecommendationCandidateProfile(["Romance", "Comedy", "Slice-of-Life"], ["cozy", "whimsical", "atmospheric"], EnergyLevel.Low, TimeOfDaySegment.Afternoon, false)),
        new(
            "music-latenight-midnight-city",
            RecommendationKind.Music,
            "Midnight City",
            "M83",
            "Spotify",
            "Nocturnal synth energy that feels made for after-hours momentum.",
            new RecommendationCandidateProfile(["Synth-Pop", "Electronic"], ["nocturnal", "dreamy", "night-drive", "moody"], EnergyLevel.Medium, TimeOfDaySegment.LateNight, false)),
        new(
            "movie-latenight-drive",
            RecommendationKind.Movie,
            "Drive",
            "Nicolas Winding Refn",
            "TMDb",
            "Stylish neo-noir mood with immersive late-night atmosphere.",
            new RecommendationCandidateProfile(["Neo-Noir", "Thriller", "Crime"], ["moody", "stylish", "immersive"], EnergyLevel.Medium, TimeOfDaySegment.LateNight, false))
    ];

    public Task<IReadOnlyList<ContentRecommendation>> GetRecommendationsAsync(
        MoodSelection selection,
        CancellationToken cancellationToken = default)
    {
        var rule = moodRuleCatalog.GetRule(selection.Mood);

        var recommendations = Seeds
            .Where(seed => selection.IncludeMusic || seed.Kind != RecommendationKind.Music)
            .Where(seed => selection.IncludeMovies || seed.Kind != RecommendationKind.Movie)
            .Where(seed => !selection.FamilyFriendlyOnly || seed.Profile.IsFamilyFriendly)
            .Select(seed => seed.ToRecommendation(moodRuleScorer.Score(selection, rule, seed.Kind, seed.Profile)))
            .Where(recommendation => recommendation.MatchScore >= MinimumDisplayScore)
            .OrderByDescending(item => item.MatchScore)
            .ToArray();

        return Task.FromResult<IReadOnlyList<ContentRecommendation>>(recommendations);
    }

    private sealed record RecommendationSeed(
        string Id,
        RecommendationKind Kind,
        string Title,
        string Creator,
        string Provider,
        string Description,
        RecommendationCandidateProfile Profile)
    {
        public ContentRecommendation ToRecommendation(MoodScoreResult result)
        {
            return new ContentRecommendation(
                Id,
                Kind,
                Title,
                Creator,
                Profile.Genres.Count > 0 ? Profile.Genres[0] : "Unknown",
                Provider,
                Description,
                result.Score,
                new RecommendationReason(result.Summary, result.Signals));
        }
    }
}
