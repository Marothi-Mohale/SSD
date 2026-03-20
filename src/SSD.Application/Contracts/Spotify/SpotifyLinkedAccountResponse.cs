namespace SSD.Application.Contracts.Spotify;

public sealed record SpotifyLinkedAccountResponse(
    bool IsLinked,
    string? SpotifyUserId,
    string? DisplayName,
    string? Email,
    string? CountryCode,
    string? SubscriptionTier,
    IReadOnlyList<string> Scopes,
    DateTimeOffset? LinkedUtc,
    DateTimeOffset? LastSyncedUtc);
