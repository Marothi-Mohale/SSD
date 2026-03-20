namespace SSD.Application.Contracts.Auth;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string DisplayName,
    string? DeviceName);
