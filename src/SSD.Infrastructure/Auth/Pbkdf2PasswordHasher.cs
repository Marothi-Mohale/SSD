using System.Security.Cryptography;

namespace SSD.Infrastructure.Auth;

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 210_000;

    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA512, KeySize);

        return $"pbkdf2-sha512${Iterations}${Convert.ToHexString(salt)}${Convert.ToHexString(hash)}";
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        var parts = passwordHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4 || !string.Equals(parts[0], "pbkdf2-sha512", StringComparison.Ordinal))
        {
            return false;
        }

        if (!int.TryParse(parts[1], out var iterations) || iterations < 100_000)
        {
            return false;
        }

        Span<byte> salt = stackalloc byte[SaltSize];
        Span<byte> expectedHash = stackalloc byte[KeySize];

        if (!Convert.TryFromHexString(parts[2], salt, out var saltBytesWritten) ||
            !Convert.TryFromHexString(parts[3], expectedHash, out var hashBytesWritten))
        {
            return false;
        }

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt[..saltBytesWritten],
            iterations,
            HashAlgorithmName.SHA512,
            hashBytesWritten);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash[..hashBytesWritten]);
    }
}
