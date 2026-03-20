using System.Text.RegularExpressions;
using SSD.Application.Exceptions;

namespace SSD.Infrastructure.Spotify;

internal static partial class SpotifyUrlParser
{
    private static readonly Regex EntityIdPattern = EntityIdRegex();

    public static string ParseTrackId(string input)
    {
        return ParseEntityId(input, "track");
    }

    public static string ParseArtistId(string input)
    {
        return ParseEntityId(input, "artist");
    }

    public static string ParsePlaylistId(string input)
    {
        return ParseEntityId(input, "playlist");
    }

    private static string ParseEntityId(string input, string entityName)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new IntegrationException("spotify_invalid_link", $"A Spotify {entityName} link is required.");
        }

        var trimmed = input.Trim();
        var uriPrefix = $"spotify:{entityName}:";
        if (trimmed.StartsWith(uriPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var uriEntityId = trimmed[uriPrefix.Length..];
            ValidateEntityId(uriEntityId, entityName);
            return uriEntityId;
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            throw new IntegrationException("spotify_malformed_link", "The Spotify link is malformed.");
        }

        if (!string.Equals(uri.Host, "open.spotify.com", StringComparison.OrdinalIgnoreCase))
        {
            throw new IntegrationException("spotify_invalid_link", $"Only Spotify {entityName} links are supported.");
        }

        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var entityIndex = Array.FindIndex(segments, segment => string.Equals(segment, entityName, StringComparison.OrdinalIgnoreCase));
        if (entityIndex < 0 || entityIndex + 1 >= segments.Length)
        {
            throw new IntegrationException("spotify_invalid_link", $"Only Spotify {entityName} links are supported.");
        }

        var pathEntityId = segments[entityIndex + 1];
        ValidateEntityId(pathEntityId, entityName);
        return pathEntityId;
    }

    private static void ValidateEntityId(string entityId, string entityName)
    {
        if (!EntityIdPattern.IsMatch(entityId))
        {
            throw new IntegrationException("spotify_malformed_link", $"The Spotify {entityName} id is malformed.");
        }
    }

    [GeneratedRegex("^[A-Za-z0-9]{22}$", RegexOptions.Compiled)]
    private static partial Regex EntityIdRegex();
}
