using Microsoft.AspNetCore.DataProtection;

namespace SSD.Infrastructure.Spotify;

internal sealed class SpotifyOAuthStateProtector(IDataProtectionProvider dataProtectionProvider)
{
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("SSD.Infrastructure.Spotify.OAuthState");

    public string Protect(Guid userId, string codeVerifier)
    {
        var payload = $"{userId:N}|{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}|{codeVerifier}";
        return _protector.Protect(payload);
    }

    public SpotifyOAuthStatePayload Unprotect(string state, TimeSpan maxAge)
    {
        var payload = _protector.Unprotect(state);
        var parts = payload.Split('|', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 3 ||
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

        if (string.IsNullOrWhiteSpace(parts[2]))
        {
            throw new InvalidOperationException("Missing Spotify PKCE verifier.");
        }

        return new SpotifyOAuthStatePayload(userId, issuedAt, parts[2]);
    }
}

internal sealed record SpotifyOAuthStatePayload(Guid UserId, DateTimeOffset IssuedUtc, string CodeVerifier);
