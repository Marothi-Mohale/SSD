namespace SSD.Application.Contracts.Spotify;

public sealed record SpotifyLinkResult(
    Guid UserId,
    string SpotifyUserId,
    string? DisplayName,
    string? Email,
    string Status,
    DateTimeOffset LinkedUtc);
