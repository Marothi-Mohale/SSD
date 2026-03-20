namespace SSD.Application.Contracts.Auth;

public sealed record RefreshTokenRequest(
    string RefreshToken,
    string? DeviceName);
