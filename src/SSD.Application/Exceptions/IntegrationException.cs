namespace SSD.Application.Exceptions;

public sealed class IntegrationException(
    string code,
    string message,
    int statusCode = 400,
    IReadOnlyList<string>? errors = null)
    : Exception(message)
{
    public string Code { get; } = code;

    public int StatusCode { get; } = statusCode;

    public IReadOnlyList<string> Errors { get; } = errors ?? [];
}
