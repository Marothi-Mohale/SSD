namespace SSD.Api.Models;

public sealed record ApiErrorResponse(
    string Message,
    IReadOnlyList<string> Errors,
    string CorrelationId);
