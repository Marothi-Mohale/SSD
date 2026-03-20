namespace SSD.Infrastructure.Spotify;

internal interface ISpotifyTokenProtector
{
    string Protect(string value);

    string Unprotect(string value);
}
