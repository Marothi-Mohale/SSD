using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SSD.Domain.Entities;

namespace SSD.Infrastructure.Auth;

public sealed class JwtTokenService(IOptions<AuthOptions> options) : ITokenService
{
    private readonly AuthOptions _options = options.Value;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public AccessTokenResult CreateAccessToken(User user, RefreshToken refreshToken, DateTimeOffset nowUtc)
    {
        var expiresUtc = nowUtc.AddMinutes(_options.AccessTokenMinutes);
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.JwtSigningKey)),
            SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
                new Claim(ClaimTypes.Name, user.DisplayName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim("sid", refreshToken.Id.ToString())
            ]),
            Issuer = _options.JwtIssuer,
            Audience = _options.JwtAudience,
            Expires = expiresUtc.UtcDateTime,
            NotBefore = nowUtc.UtcDateTime,
            SigningCredentials = signingCredentials
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return new AccessTokenResult(_tokenHandler.WriteToken(token), expiresUtc);
    }

    public RefreshTokenResult CreateRefreshToken(
        Guid userId,
        string? deviceName,
        string? userAgent,
        string? ipAddress,
        DateTimeOffset nowUtc)
    {
        var plainTextToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshToken = new RefreshToken(Guid.NewGuid(), userId, ComputeRefreshTokenHash(plainTextToken), nowUtc.AddDays(_options.RefreshTokenDays));
        refreshToken.AttachSessionMetadata(deviceName, userAgent, ipAddress);

        return new RefreshTokenResult(plainTextToken, refreshToken, refreshToken.ExpiresUtc);
    }

    public string ComputeRefreshTokenHash(string refreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
    }
}
