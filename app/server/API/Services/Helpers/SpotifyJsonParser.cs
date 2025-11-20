using System.Text.Json;
using API.DTOs.Spotify;

namespace API.Services.Helpers;

public static class SpotifyJsonParser {
    public static SpotifyTrack ParseTrack(JsonElement track) {
        return new SpotifyTrack(
            track.GetProperty("id").GetString()!,
            track.GetProperty("name").GetString()!,
            track.GetProperty("uri").GetString()!,
            track.GetProperty("artists").EnumerateArray().Select(a => new SpotifyArtist(
                a.GetProperty("id").GetString()!,
                a.GetProperty("name").GetString()!,
                a.GetProperty("uri").GetString()!
            )).ToArray(),
            ParseAlbum(track.GetProperty("album")),
            track.GetProperty("duration_ms").GetInt32(),
            track.GetProperty("popularity").GetInt32()
        );
    }

    public static SpotifyAlbum ParseAlbum(JsonElement album) {
        return new SpotifyAlbum(
            album.GetProperty("id").GetString()!,
            album.GetProperty("name").GetString()!,
            album.GetProperty("uri").GetString()!,
            album.GetProperty("images").EnumerateArray().Select(i => new SpotifyImage(
                i.GetProperty("url").GetString()!,
                i.GetProperty("height").GetInt32(),
                i.GetProperty("width").GetInt32()
            )).ToArray()
        );
    }

    public static AudioFeatures ParseAudioFeatures(JsonElement af) {
        return new AudioFeatures(
            (float)af.GetProperty("danceability").GetDouble(),
            (float)af.GetProperty("energy").GetDouble(),
            af.GetProperty("key").GetInt32(),
            (float)af.GetProperty("loudness").GetDouble(),
            af.GetProperty("mode").GetInt32(),
            (float)af.GetProperty("speechiness").GetDouble(),
            (float)af.GetProperty("acousticness").GetDouble(),
            (float)af.GetProperty("instrumentalness").GetDouble(),
            (float)af.GetProperty("liveness").GetDouble(),
            (float)af.GetProperty("valence").GetDouble(),
            (float)af.GetProperty("tempo").GetDouble()
        );
    }
}