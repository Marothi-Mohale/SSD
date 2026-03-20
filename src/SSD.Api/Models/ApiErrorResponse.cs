namespace SSD.Api.Models;

public sealed record ApiErrorResponse(
    string Code,
    string Message,
    IReadOnlyList<string> Errors,
    string CorrelationId);
