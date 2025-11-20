using System.Net.Http.Headers;
using System.Text.Json;
using API.DTOs.Spotify;
using API.Interfaces;
using API.Services.Helpers;

namespace API.Services.Spotify;

public class SpotifyTrackService : ISpotifyTrackService {
    private readonly HttpClient _httpClient;
    private readonly ILogger<SpotifyTrackService> _logger;
    private const string SpotifyApiBaseUrl = "https://api.spotify.com/v1";

    public SpotifyTrackService(HttpClient httpClient, ILogger<SpotifyTrackService> logger) {
        _httpClient = httpClient;
        _logger = logger;
    }

    private void SetAuthorizationHeader(string accessToken) {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public async Task<SpotifyTrack[]> SearchTracksAsync(string accessToken, string query, int limit = 20) {
        SetAuthorizationHeader(accessToken);

        var encodedQuery = Uri.EscapeDataString(query);
        var url = $"{SpotifyApiBaseUrl}/search?q={encodedQuery}&type=track&limit={limit}";

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode) {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Spotify search request failed with status {StatusCode}. Query: {Query}. Error: {Error}",
                response.StatusCode, query, errorContent);
        }

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var searchResult = JsonSerializer.Deserialize<JsonElement>(content);
        var items = searchResult.GetProperty("tracks").GetProperty("items");

        var tracks = items.EnumerateArray().Select(SpotifyJsonParser.ParseTrack).ToArray();

        _logger.LogInformation("Spotify search for '{Query}' returned {Count} tracks", query, tracks.Length);

        return tracks;
    }

    public async Task<SpotifyTrack[]> GetRecommendationsAsync(string accessToken, string[] seedTracks, int limit = 20) {
        SetAuthorizationHeader(accessToken);

        var seeds = string.Join(",", seedTracks.Take(5));
        var url = $"{SpotifyApiBaseUrl}/recommendations?seed_tracks={seeds}&limit={limit}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var recommendationsResult = JsonSerializer.Deserialize<JsonElement>(content);
        var tracks = recommendationsResult.GetProperty("tracks");

        return tracks.EnumerateArray().Select(SpotifyJsonParser.ParseTrack).ToArray();
    }

    public async Task<AudioFeatures[]> GetAudioFeaturesAsync(string accessToken, string[] trackIds) {
        SetAuthorizationHeader(accessToken);

        if (trackIds.Length > 100) {
            _logger.LogWarning("Received {Count} track IDs, processing in batches", trackIds.Length);
            var allFeatures = new List<AudioFeatures>();

            for (int i = 0; i < trackIds.Length; i += 100) {
                var batch = trackIds.Skip(i).Take(100).ToArray();
                var batchFeatures = await GetAudioFeaturesAsync(accessToken, batch);
                allFeatures.AddRange(batchFeatures);
            }

            return allFeatures.ToArray();
        }

        var ids = string.Join(",", trackIds);
        var url = $"{SpotifyApiBaseUrl}/audio-features?ids={ids}";

        _logger.LogInformation("Fetching audio features for {Count} tracks", trackIds.Length);
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode) {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Spotify audio features request failed with status {StatusCode}. Error: {Error}",
                response.StatusCode, errorContent);
        }

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        var audioFeatures = result.GetProperty("audio_features");

        return audioFeatures.EnumerateArray()
            .Where(af => af.ValueKind != JsonValueKind.Null)
            .Select(SpotifyJsonParser.ParseAudioFeatures)
            .ToArray();
    }

    public async Task<SpotifyTrack[]> GetUserSavedTracksAsync(string accessToken, int limit = 50) {
        SetAuthorizationHeader(accessToken);

        var url = $"{SpotifyApiBaseUrl}/me/tracks?limit={limit}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        var items = result.GetProperty("items");

        return items.EnumerateArray()
            .Select(item => SpotifyJsonParser.ParseTrack(item.GetProperty("track")))
            .ToArray();
    }

    public async Task<SpotifyPlaylistTrack[]> GetPlaylistTracksAsync(string accessToken, string playlistId) {
        SetAuthorizationHeader(accessToken);

        var allTracks = new List<SpotifyPlaylistTrack>();
        var url = $"{SpotifyApiBaseUrl}/playlists/{playlistId}/tracks?limit=100";

        do {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);
            var items = result.GetProperty("items");

            foreach (var item in items.EnumerateArray()) {
                var track = item.GetProperty("track");
                if (track.ValueKind == JsonValueKind.Null) continue;

                var spotifyTrack = SpotifyJsonParser.ParseTrack(track);
                var addedAt = item.GetProperty("added_at").GetString();

                allTracks.Add(new SpotifyPlaylistTrack(
                    spotifyTrack.Id,
                    spotifyTrack.Name,
                    spotifyTrack.Artists,
                    spotifyTrack.Album,
                    spotifyTrack.Uri,
                    spotifyTrack.Popularity,
                    addedAt ?? DateTime.UtcNow.ToString("o")
                ));
            }

            url = result.TryGetProperty("next", out var next) && next.ValueKind != JsonValueKind.Null
                ? next.GetString()!
                : null;

        } while (url != null);

        _logger.LogInformation("Retrieved {Count} tracks from playlist {PlaylistId}", allTracks.Count, playlistId);

        return allTracks.ToArray();
    }
}