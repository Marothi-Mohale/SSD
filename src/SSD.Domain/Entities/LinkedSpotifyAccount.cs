using SSD.Domain.Common;
using SSD.Domain.Enums;

namespace SSD.Domain.Entities;

public sealed class LinkedSpotifyAccount : AuditableEntity
{
    public LinkedSpotifyAccount()
    {
        SpotifyUserId = string.Empty;
        EncryptedRefreshToken = string.Empty;
        Status = SpotifyLinkStatus.Pending;
        GrantedScopes = [];
    }

    public LinkedSpotifyAccount(Guid id, Guid userId, string spotifyUserId, string encryptedRefreshToken)
        : base(id)
    {
        UserId = userId;
        SpotifyUserId = spotifyUserId;
        EncryptedRefreshToken = encryptedRefreshToken;
        Status = SpotifyLinkStatus.Active;
        LinkedUtc = CreatedUtc;
        GrantedScopes = [];
    }

    public Guid UserId { get; private set; }

    public string SpotifyUserId { get; private set; }

    public string? SpotifyDisplayName { get; private set; }

    public string? CountryCode { get; private set; }

    public string? SubscriptionTier { get; private set; }

    public string EncryptedRefreshToken { get; private set; }

    public DateTimeOffset? AccessTokenExpiresUtc { get; private set; }

    public SpotifyLinkStatus Status { get; private set; }

    public DateTimeOffset LinkedUtc { get; private set; }

    public DateTimeOffset? LastSyncedUtc { get; private set; }

    public DateTimeOffset? RevokedUtc { get; private set; }

    public List<string> GrantedScopes { get; private set; }

    public User? User { get; private set; }
}
