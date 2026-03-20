using SSD.Domain.Common;
using SSD.Domain.Enums;

namespace SSD.Domain.Entities;

public sealed class MoodProfile : AuditableEntity
{
    public MoodProfile()
    {
        Name = string.Empty;
        PreferredGenres = [];
        AvoidedGenres = [];
    }

    public MoodProfile(Guid id, Guid userId, string name, MoodCategory mood)
        : base(id)
    {
        UserId = userId;
        Name = name;
        Mood = mood;
        PreferredGenres = [];
        AvoidedGenres = [];
    }

    public Guid UserId { get; private set; }

    public string Name { get; private set; }

    public MoodCategory Mood { get; private set; }

    public EnergyLevel? EnergyLevel { get; private set; }

    public TimeOfDaySegment? TimeOfDay { get; private set; }

    public bool FamilyFriendlyOnly { get; private set; }

    public bool IncludeMusic { get; private set; } = true;

    public bool IncludeMovies { get; private set; } = true;

    public bool IsDefault { get; private set; }

    public string? Notes { get; private set; }

    public List<string> PreferredGenres { get; private set; }

    public List<string> AvoidedGenres { get; private set; }

    public User? User { get; private set; }
}
