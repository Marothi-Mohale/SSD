using SSD.Application.Contracts.Spotify;

namespace SSD.Infrastructure.Spotify;

internal static class SpotifyMapper
{
    public static SpotifyTrackResponse MapTrack(SpotifyTrackApiResponse track, string? market = null)
    {
        return new SpotifyTrackResponse(
            track.Id,
            track.Name,
            track.Artists.Select(artist => artist.Name).Where(name => !string.IsNullOrWhiteSpace(name)).ToArray(),
            track.Album?.Name ?? string.Empty,
            track.Album?.Images.OrderByDescending(image => image.Width ?? 0).Select(image => image.Url).FirstOrDefault(),
            track.DurationMilliseconds,
            track.ExternalUrls?.Spotify ?? $"https://open.spotify.com/track/{track.Id}",
            track.PreviewUrl,
            track.IsPlayable ?? true,
            track.Explicit,
            market);
    }

    public static SpotifyTrackSummaryResponse MapTrackSummary(SpotifyTrackApiResponse track)
    {
        return new SpotifyTrackSummaryResponse(
            track.Id,
            track.Name,
            track.Artists.Select(artist => artist.Name).Where(name => !string.IsNullOrWhiteSpace(name)).ToArray(),
            track.Album?.Name ?? string.Empty,
            track.Album?.Images.OrderByDescending(image => image.Width ?? 0).Select(image => image.Url).FirstOrDefault(),
            track.ExternalUrls?.Spotify ?? $"https://open.spotify.com/track/{track.Id}",
            track.PreviewUrl);
    }

    public static SpotifyArtistResponse MapArtist(SpotifyArtistApiResponse artist)
    {
        return new SpotifyArtistResponse(
            artist.Id,
            artist.Name,
            artist.Genres.Where(genre => !string.IsNullOrWhiteSpace(genre)).ToArray(),
            artist.Popularity,
            artist.Followers?.Total ?? 0,
            artist.Images.OrderByDescending(image => image.Width ?? 0).Select(image => image.Url).FirstOrDefault(),
            artist.ExternalUrls?.Spotify ?? $"https://open.spotify.com/artist/{artist.Id}");
    }

    public static SpotifyPlaylistResponse MapPlaylist(SpotifyPlaylistApiResponse playlist)
    {
        return new SpotifyPlaylistResponse(
            playlist.Id,
            playlist.Name,
            playlist.Description,
            playlist.Owner?.DisplayName ?? "Spotify",
            playlist.Tracks?.Total ?? 0,
            playlist.Images.OrderByDescending(image => image.Width ?? 0).Select(image => image.Url).FirstOrDefault(),
            playlist.ExternalUrls?.Spotify ?? $"https://open.spotify.com/playlist/{playlist.Id}",
            playlist.Collaborative,
            playlist.Public ?? false,
            playlist.Tracks?.Items
                .Select(item => item.Track)
                .Where(track => track is not null)
                .Select(track => MapTrackSummary(track!))
                .ToArray() ?? []);
    }
}
