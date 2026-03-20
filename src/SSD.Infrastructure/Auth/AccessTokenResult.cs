namespace SSD.Infrastructure.Auth;

public sealed record AccessTokenResult(
    string Token,
    DateTimeOffset ExpiresUtc);
