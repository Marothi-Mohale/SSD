namespace SSD.Application.Contracts.Auth;

public sealed record LogoutRequest(
    string RefreshToken,
    bool LogoutFromAllDevices = false);
