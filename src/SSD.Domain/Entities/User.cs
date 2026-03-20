using SSD.Domain.Common;
using SSD.Domain.Enums;
using SSD.Domain.ValueObjects;

namespace SSD.Domain.Entities;

public sealed class User : AggregateRoot
{
    private readonly List<FavoriteItem> _favoriteItems = [];
    private readonly List<MoodProfile> _moodProfiles = [];
    private readonly List<RefreshToken> _refreshTokens = [];
    private readonly List<SearchHistory> _searchHistoryEntries = [];

    public User()
    {
        Email = new EmailAddress("placeholder@example.com");
        DisplayName = string.Empty;
        PasswordHash = string.Empty;
        PasswordHashAlgorithm = string.Empty;
        Status = UserStatus.Active;
    }

    public User(Guid id, EmailAddress email, string displayName, string passwordHash, string passwordHashAlgorithm)
        : base(id)
    {
        Email = email;
        DisplayName = displayName;
        PasswordHash = passwordHash;
        PasswordHashAlgorithm = passwordHashAlgorithm;
        Status = UserStatus.Active;
    }

    public EmailAddress Email { get; private set; }

    public string DisplayName { get; private set; }

    public string PasswordHash { get; private set; }

    public string PasswordHashAlgorithm { get; private set; }

    public UserStatus Status { get; private set; }

    public DateTimeOffset? EmailConfirmedUtc { get; private set; }

    public DateTimeOffset? LastLoginUtc { get; private set; }

    public UserPreference? Preference { get; private set; }

    public LinkedSpotifyAccount? LinkedSpotifyAccount { get; private set; }

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens;

    public IReadOnlyCollection<MoodProfile> MoodProfiles => _moodProfiles;

    public IReadOnlyCollection<FavoriteItem> FavoriteItems => _favoriteItems;

    public IReadOnlyCollection<SearchHistory> SearchHistoryEntries => _searchHistoryEntries;
}
