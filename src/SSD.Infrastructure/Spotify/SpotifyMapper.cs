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
}
