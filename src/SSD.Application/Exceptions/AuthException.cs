namespace SSD.Application.Exceptions;

public sealed class AuthException(string code, string message, IReadOnlyList<string>? errors = null)
    : Exception(message)
{
    public string Code { get; } = code;

    public IReadOnlyList<string> Errors { get; } = errors ?? [];
}
