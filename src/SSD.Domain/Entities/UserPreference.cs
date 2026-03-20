using SSD.Domain.Common;
using SSD.Domain.Enums;

namespace SSD.Domain.Entities;

public sealed class UserPreference : AuditableEntity
{
    public UserPreference()
    {
        PreferredLanguageCode = "en";
        PreferredRegionCode = "US";
    }

    public UserPreference(Guid id, Guid userId)
        : base(id)
    {
        UserId = userId;
        PreferredLanguageCode = "en";
        PreferredRegionCode = "US";
    }

    public Guid UserId { get; private set; }

    public bool IncludeMusicByDefault { get; private set; } = true;

    public bool IncludeMoviesByDefault { get; private set; } = true;

    public bool FamilyFriendlyOnly { get; private set; }

    public bool AllowExplicitContent { get; private set; } = true;

    public string PreferredLanguageCode { get; private set; }

    public string PreferredRegionCode { get; private set; }

    public EnergyLevel? DefaultEnergyLevel { get; private set; }

    public TimeOfDaySegment? DefaultTimeOfDay { get; private set; }

    public DateTimeOffset? OnboardingCompletedUtc { get; private set; }

    public User? User { get; private set; }
}
