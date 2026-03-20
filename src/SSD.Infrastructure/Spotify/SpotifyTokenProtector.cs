using Microsoft.AspNetCore.DataProtection;

namespace SSD.Infrastructure.Spotify;

internal sealed class SpotifyTokenProtector(IDataProtectionProvider dataProtectionProvider) : ISpotifyTokenProtector
{
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("SSD.Infrastructure.Spotify.Tokens");

    public string Protect(string value) => _protector.Protect(value);

    public string Unprotect(string value) => _protector.Unprotect(value);
}
