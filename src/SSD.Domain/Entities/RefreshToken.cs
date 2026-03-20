using SSD.Domain.Common;

namespace SSD.Domain.Entities;

public sealed class RefreshToken : AuditableEntity
{
    public RefreshToken()
    {
        TokenHash = string.Empty;
        DeviceName = string.Empty;
        UserAgent = string.Empty;
        CreatedByIp = string.Empty;
    }

    public RefreshToken(Guid id, Guid userId, string tokenHash, DateTimeOffset expiresUtc)
        : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresUtc = expiresUtc;
        DeviceName = string.Empty;
        UserAgent = string.Empty;
        CreatedByIp = string.Empty;
    }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; }

    public DateTimeOffset ExpiresUtc { get; private set; }

    public DateTimeOffset? RevokedUtc { get; private set; }

    public DateTimeOffset? LastUsedUtc { get; private set; }

    public string DeviceName { get; private set; }

    public string UserAgent { get; private set; }

    public string CreatedByIp { get; private set; }

    public string? RevokedByIp { get; private set; }

    public string? ReplacedByTokenHash { get; private set; }

    public string? RevocationReason { get; private set; }

    public User? User { get; private set; }

    public bool IsActive(DateTimeOffset nowUtc) => RevokedUtc is null && ExpiresUtc > nowUtc;

    public void AttachSessionMetadata(string? deviceName, string? userAgent, string? createdByIp)
    {
        DeviceName = deviceName?.Trim() ?? string.Empty;
        UserAgent = userAgent?.Trim() ?? string.Empty;
        CreatedByIp = createdByIp?.Trim() ?? string.Empty;
        Touch();
    }

    public void MarkUsed(DateTimeOffset? lastUsedUtc = null)
    {
        LastUsedUtc = lastUsedUtc ?? DateTimeOffset.UtcNow;
        Touch(LastUsedUtc);
    }

    public void Revoke(
        string? revokedByIp,
        string? reason,
        string? replacedByTokenHash = null,
        DateTimeOffset? revokedUtc = null)
    {
        RevokedUtc = revokedUtc ?? DateTimeOffset.UtcNow;
        RevokedByIp = revokedByIp?.Trim();
        RevocationReason = reason?.Trim();
        ReplacedByTokenHash = replacedByTokenHash?.Trim();
        Touch(RevokedUtc);
    }
}
