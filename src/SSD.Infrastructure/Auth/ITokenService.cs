using SSD.Domain.Entities;

namespace SSD.Infrastructure.Auth;

public interface ITokenService
{
    AccessTokenResult CreateAccessToken(User user, RefreshToken refreshToken, DateTimeOffset nowUtc);

    RefreshTokenResult CreateRefreshToken(Guid userId, string? deviceName, string? userAgent, string? ipAddress, DateTimeOffset nowUtc);

    string ComputeRefreshTokenHash(string refreshToken);
}
