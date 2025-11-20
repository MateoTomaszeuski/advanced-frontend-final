using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using API.DTOs.Spotify;
using API.Interfaces;
using API.Services.Helpers;

namespace API.Services.Spotify;

public class SpotifyPlaylistService : ISpotifyPlaylistService {
    private readonly HttpClient _httpClient;
    private readonly ILogger<SpotifyPlaylistService> _logger;
    private const string SpotifyApiBaseUrl = "https://api.spotify.com/v1";

    public SpotifyPlaylistService(HttpClient httpClient, ILogger<SpotifyPlaylistService> logger) {
        _httpClient = httpClient;
        _logger = logger;
    }

    private void SetAuthorizationHeader(string accessToken) {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public async Task<SpotifyPlaylist> CreatePlaylistAsync(string accessToken, string userId, CreatePlaylistRequest request) {
        SetAuthorizationHeader(accessToken);

        var payload = new {
            name = request.Name,
            description = request.Description ?? "",
            @public = request.Public
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{SpotifyApiBaseUrl}/users/{userId}/playlists", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var playlistJson = JsonSerializer.Deserialize<JsonElement>(responseContent);

        return new SpotifyPlaylist(
            playlistJson.GetProperty("id").GetString()!,
            playlistJson.GetProperty("name").GetString()!,
            playlistJson.TryGetProperty("description", out var desc) ? desc.GetString() : null,
            playlistJson.GetProperty("uri").GetString()!,
            playlistJson.GetProperty("tracks").GetProperty("total").GetInt32()
        );
    }

    public async Task<SpotifyPlaylist> GetPlaylistAsync(string accessToken, string playlistId) {
        SetAuthorizationHeader(accessToken);

        var response = await _httpClient.GetAsync($"{SpotifyApiBaseUrl}/playlists/{playlistId}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var playlistJson = JsonSerializer.Deserialize<JsonElement>(content);

        return new SpotifyPlaylist(
            playlistJson.GetProperty("id").GetString()!,
            playlistJson.GetProperty("name").GetString()!,
            playlistJson.TryGetProperty("description", out var desc) ? desc.GetString() : null,
            playlistJson.GetProperty("uri").GetString()!,
            playlistJson.GetProperty("tracks").GetProperty("total").GetInt32()
        );
    }

    public async Task<SpotifyPlaylist[]> GetUserPlaylistsAsync(string accessToken, int limit = 50) {
        SetAuthorizationHeader(accessToken);

        var allPlaylists = new List<SpotifyPlaylist>();
        var url = $"{SpotifyApiBaseUrl}/me/playlists?limit=50&offset=0";

        do {
            _logger.LogInformation("Fetching playlists from: {Url}", url);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);
            var items = result.GetProperty("items");

            var hasNext = result.TryGetProperty("next", out var next) && next.ValueKind != JsonValueKind.Null;

            foreach (var item in items.EnumerateArray()) {
                allPlaylists.Add(new SpotifyPlaylist(
                    item.GetProperty("id").GetString()!,
                    item.GetProperty("name").GetString()!,
                    item.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                    item.GetProperty("uri").GetString()!,
                    item.GetProperty("tracks").GetProperty("total").GetInt32()
                ));
            }

            url = hasNext ? next.GetString() : null;

        } while (url != null);

        _logger.LogInformation("Retrieved {Count} playlists for user", allPlaylists.Count);

        return allPlaylists.ToArray();
    }

    public async Task AddTracksToPlaylistAsync(string accessToken, string playlistId, string[] trackUris) {
        SetAuthorizationHeader(accessToken);

        const int batchSize = 100;

        for (int i = 0; i < trackUris.Length; i += batchSize) {
            var batch = trackUris.Skip(i).Take(batchSize).ToArray();
            var payload = new { uris = batch };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{SpotifyApiBaseUrl}/playlists/{playlistId}/tracks", content);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Added {Count} tracks to playlist {PlaylistId} (batch {BatchNum})",
                batch.Length, playlistId, (i / batchSize) + 1);
        }
    }

    public async Task RemoveTracksFromPlaylistAsync(string accessToken, string playlistId, string[] trackUris) {
        SetAuthorizationHeader(accessToken);

        var tracks = trackUris.Select(uri => new { uri }).ToArray();
        var payload = new { tracks };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Delete, $"{SpotifyApiBaseUrl}/playlists/{playlistId}/tracks") {
            Content = content
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Removed {Count} tracks from playlist {PlaylistId}", trackUris.Length, playlistId);
    }
}