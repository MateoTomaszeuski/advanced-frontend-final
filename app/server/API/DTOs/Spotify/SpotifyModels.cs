namespace API.DTOs.Spotify;

public record SpotifyTrack(
    string Id,
    string Name,
    string Uri,
    SpotifyArtist[] Artists,
    SpotifyAlbum Album,
    int DurationMs,
    int Popularity,
    AudioFeatures? AudioFeatures = null
);

public record SpotifyArtist(
    string Id,
    string Name,
    string Uri
);

public record SpotifyAlbum(
    string Id,
    string Name,
    string Uri,
    SpotifyImage[] Images
);

public record SpotifyImage(
    string Url,
    int Height,
    int Width
);

public record AudioFeatures(
    float Danceability,
    float Energy,
    int Key,
    float Loudness,
    int Mode,
    float Speechiness,
    float Acousticness,
    float Instrumentalness,
    float Liveness,
    float Valence,
    float Tempo
);

public record SpotifyPlaylist(
    string Id,
    string Name,
    string? Description,
    string Uri,
    int TotalTracks
);

public record SpotifyPlaylistTrack(
    string Id,
    string Name,
    SpotifyArtist[] Artists,
    SpotifyAlbum Album,
    string Uri,
    int Popularity,
    string AddedAt
);

public record CreatePlaylistRequest(
    string Name,
    string? Description = null,
    bool Public = false
);

public record AddTracksToPlaylistRequest(
    string[] TrackUris
);
