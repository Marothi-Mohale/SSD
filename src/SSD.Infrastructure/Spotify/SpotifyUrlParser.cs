using System.Text.RegularExpressions;
using SSD.Application.Exceptions;

namespace SSD.Infrastructure.Spotify;

internal static partial class SpotifyUrlParser
{
    private static readonly Regex TrackIdPattern = TrackIdRegex();

    public static string ParseTrackId(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new IntegrationException("spotify_invalid_link", "A Spotify track link is required.");
        }

        var trimmed = input.Trim();
        if (trimmed.StartsWith("spotify:track:", StringComparison.OrdinalIgnoreCase))
        {
            var uriTrackId = trimmed["spotify:track:".Length..];
            ValidateTrackId(uriTrackId);
            return uriTrackId;
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            throw new IntegrationException("spotify_malformed_link", "The Spotify link is malformed.");
        }

        if (!string.Equals(uri.Host, "open.spotify.com", StringComparison.OrdinalIgnoreCase))
        {
            throw new IntegrationException("spotify_invalid_link", "Only Spotify track links are supported.");
        }

        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var trackIndex = Array.FindIndex(segments, segment => string.Equals(segment, "track", StringComparison.OrdinalIgnoreCase));
        if (trackIndex < 0 || trackIndex + 1 >= segments.Length)
        {
            throw new IntegrationException("spotify_invalid_link", "Only Spotify track links are supported.");
        }

        var trackId = segments[trackIndex + 1];
        ValidateTrackId(trackId);
        return trackId;
    }

    private static void ValidateTrackId(string trackId)
    {
        if (!TrackIdPattern.IsMatch(trackId))
        {
            throw new IntegrationException("spotify_malformed_link", "The Spotify track id is malformed.");
        }
    }

    [GeneratedRegex("^[A-Za-z0-9]{22}$", RegexOptions.Compiled)]
    private static partial Regex TrackIdRegex();
}
