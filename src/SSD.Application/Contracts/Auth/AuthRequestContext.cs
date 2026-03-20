namespace SSD.Application.Contracts.Auth;

public sealed record AuthRequestContext(
    string? IpAddress,
    string? UserAgent,
    string? CorrelationId);
