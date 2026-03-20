using Microsoft.AspNetCore.DataProtection;

namespace SSD.Infrastructure.Spotify;

internal sealed class SpotifyOAuthStateProtector(IDataProtectionProvider dataProtectionProvider)
{
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("SSD.Infrastructure.Spotify.OAuthState");

    public string Protect(Guid userId)
    {
        var payload = $"{userId:N}|{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        return _protector.Protect(payload);
    }

    public Guid Unprotect(string state, TimeSpan maxAge)
    {
        var payload = _protector.Unprotect(state);
        var parts = payload.Split('|', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2 ||
            !Guid.TryParseExact(parts[0], "N", out var userId) ||
            !long.TryParse(parts[1], out var issuedAtSeconds))
        {
            throw new InvalidOperationException("Malformed Spotify OAuth state.");
        }

        var issuedAt = DateTimeOffset.FromUnixTimeSeconds(issuedAtSeconds);
        if (DateTimeOffset.UtcNow - issuedAt > maxAge)
        {
            throw new InvalidOperationException("Expired Spotify OAuth state.");
        }

        return userId;
    }
}
