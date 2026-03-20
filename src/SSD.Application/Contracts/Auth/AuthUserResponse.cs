namespace SSD.Application.Contracts.Auth;

public sealed record AuthUserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string Role);
