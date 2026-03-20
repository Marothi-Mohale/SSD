namespace SSD.Application.Contracts.Auth;

public sealed record AuthResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresUtc,
    AuthUserResponse User);
