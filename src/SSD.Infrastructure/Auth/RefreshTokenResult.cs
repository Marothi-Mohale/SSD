using SSD.Domain.Entities;

namespace SSD.Infrastructure.Auth;

public sealed record RefreshTokenResult(
    string PlainTextToken,
    RefreshToken Token,
    DateTimeOffset ExpiresUtc);
