using System.Text.Json.Serialization;

namespace SSD.Infrastructure.Spotify;

public sealed class SpotifyTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
}

public sealed class SpotifyCurrentUserResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("product")]
    public string? Product { get; set; }
}

public sealed class SpotifyTrackApiResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("duration_ms")]
    public int DurationMilliseconds { get; set; }

    [JsonPropertyName("preview_url")]
    public string? PreviewUrl { get; set; }

    [JsonPropertyName("explicit")]
    public bool Explicit { get; set; }

    [JsonPropertyName("is_playable")]
    public bool? IsPlayable { get; set; }

    [JsonPropertyName("external_urls")]
    public SpotifyExternalUrls? ExternalUrls { get; set; }

    [JsonPropertyName("artists")]
    public List<SpotifyArtist> Artists { get; set; } = [];

    [JsonPropertyName("album")]
    public SpotifyAlbum? Album { get; set; }
}

public sealed class SpotifyExternalUrls
{
    [JsonPropertyName("spotify")]
    public string? Spotify { get; set; }
}

public sealed class SpotifyArtist
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public sealed class SpotifyAlbum
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("images")]
    public List<SpotifyImage> Images { get; set; } = [];
}

public sealed class SpotifyImage
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("width")]
    public int? Width { get; set; }
}

public sealed class SpotifyArtistApiResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("genres")]
    public List<string> Genres { get; set; } = [];

    [JsonPropertyName("popularity")]
    public int Popularity { get; set; }

    [JsonPropertyName("followers")]
    public SpotifyFollowers? Followers { get; set; }

    [JsonPropertyName("images")]
    public List<SpotifyImage> Images { get; set; } = [];

    [JsonPropertyName("external_urls")]
    public SpotifyExternalUrls? ExternalUrls { get; set; }
}

public sealed class SpotifyFollowers
{
    [JsonPropertyName("total")]
    public int Total { get; set; }
}

public sealed class SpotifyPlaylistApiResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("collaborative")]
    public bool Collaborative { get; set; }

    [JsonPropertyName("public")]
    public bool? Public { get; set; }

    [JsonPropertyName("owner")]
    public SpotifyPlaylistOwner? Owner { get; set; }

    [JsonPropertyName("images")]
    public List<SpotifyImage> Images { get; set; } = [];

    [JsonPropertyName("external_urls")]
    public SpotifyExternalUrls? ExternalUrls { get; set; }

    [JsonPropertyName("tracks")]
    public SpotifyPlaylistTracks? Tracks { get; set; }
}

public sealed class SpotifyPlaylistOwner
{
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }
}

public sealed class SpotifyPlaylistTracks
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("items")]
    public List<SpotifyPlaylistTrackItem> Items { get; set; } = [];
}

public sealed class SpotifyPlaylistTrackItem
{
    [JsonPropertyName("track")]
    public SpotifyTrackApiResponse? Track { get; set; }
}

public sealed class SpotifyPagingResponse<T>
{
    [JsonPropertyName("items")]
    public List<T> Items { get; set; } = [];
}
