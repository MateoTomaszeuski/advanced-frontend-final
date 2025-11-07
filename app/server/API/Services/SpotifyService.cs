using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using API.DTOs.Spotify;
using API.Services.Helpers;

namespace API.Services;

public interface ISpotifyService
{
    Task<SpotifyPlaylist> CreatePlaylistAsync(string accessToken, string userId, CreatePlaylistRequest request);
    Task<SpotifyTrack[]> SearchTracksAsync(string accessToken, string query, int limit = 20);
    Task<SpotifyTrack[]> GetRecommendationsAsync(string accessToken, string[] seedTracks, int limit = 20);
    Task<AudioFeatures[]> GetAudioFeaturesAsync(string accessToken, string[] trackIds);
    Task<SpotifyTrack[]> GetUserSavedTracksAsync(string accessToken, int limit = 50);
    Task AddTracksToPlaylistAsync(string accessToken, string playlistId, string[] trackUris);
    Task<string> GetCurrentUserIdAsync(string accessToken);
    Task<SpotifyUserProfile> GetCurrentUserProfileAsync(string accessToken);
    Task<SpotifyPlaylistTrack[]> GetPlaylistTracksAsync(string accessToken, string playlistId);
    Task<SpotifyPlaylist> GetPlaylistAsync(string accessToken, string playlistId);
    Task RemoveTracksFromPlaylistAsync(string accessToken, string playlistId, string[] trackUris);
    Task<SpotifyPlaylist[]> GetUserPlaylistsAsync(string accessToken, int limit = 50);
}

public class SpotifyService : ISpotifyService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SpotifyService> _logger;
    private const string SpotifyApiBaseUrl = "https://api.spotify.com/v1";

    public SpotifyService(HttpClient httpClient, ILogger<SpotifyService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    private void SetAuthorizationHeader(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public async Task<string> GetCurrentUserIdAsync(string accessToken)
    {
        SetAuthorizationHeader(accessToken);
        var response = await _httpClient.GetAsync($"{SpotifyApiBaseUrl}/me");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var userProfile = JsonSerializer.Deserialize<JsonElement>(content);
        return userProfile.GetProperty("id").GetString() ?? throw new Exception("Could not get user ID");
    }

    public async Task<SpotifyUserProfile> GetCurrentUserProfileAsync(string accessToken)
    {
        SetAuthorizationHeader(accessToken);
        var response = await _httpClient.GetAsync($"{SpotifyApiBaseUrl}/me");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var userJson = JsonSerializer.Deserialize<JsonElement>(content);

        var id = userJson.GetProperty("id").GetString() ?? "";
        var displayName = userJson.GetProperty("display_name").GetString() ?? "";
        var email = userJson.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;
        var country = userJson.TryGetProperty("country", out var countryProp) ? countryProp.GetString() : null;
        
        string? imageUrl = null;
        if (userJson.TryGetProperty("images", out var images) && images.GetArrayLength() > 0)
        {
            imageUrl = images[0].GetProperty("url").GetString();
        }

        return new SpotifyUserProfile(id, displayName, email ?? "", country, imageUrl);
    }

    public async Task<SpotifyPlaylist> CreatePlaylistAsync(string accessToken, string userId, CreatePlaylistRequest request)
    {
        SetAuthorizationHeader(accessToken);

        var payload = new
        {
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

    public async Task<SpotifyTrack[]> SearchTracksAsync(string accessToken, string query, int limit = 20)
    {
        SetAuthorizationHeader(accessToken);

        var encodedQuery = Uri.EscapeDataString(query);
        var url = $"{SpotifyApiBaseUrl}/search?q={encodedQuery}&type=track&limit={limit}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var searchResult = JsonSerializer.Deserialize<JsonElement>(content);
        var items = searchResult.GetProperty("tracks").GetProperty("items");

        var tracks = items.EnumerateArray().Select(SpotifyJsonParser.ParseTrack).ToArray();
        
        _logger.LogInformation("Spotify search for '{Query}' returned {Count} tracks", query, tracks.Length);
        
        return tracks;
    }

    public async Task<SpotifyTrack[]> GetRecommendationsAsync(string accessToken, string[] seedTracks, int limit = 20)
    {
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

    public async Task<AudioFeatures[]> GetAudioFeaturesAsync(string accessToken, string[] trackIds)
    {
        SetAuthorizationHeader(accessToken);

        var ids = string.Join(",", trackIds);
        var url = $"{SpotifyApiBaseUrl}/audio-features?ids={ids}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        var audioFeatures = result.GetProperty("audio_features");

        return audioFeatures.EnumerateArray()
            .Where(af => af.ValueKind != JsonValueKind.Null)
            .Select(SpotifyJsonParser.ParseAudioFeatures)
            .ToArray();
    }

    public async Task<SpotifyTrack[]> GetUserSavedTracksAsync(string accessToken, int limit = 50)
    {
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

    public async Task AddTracksToPlaylistAsync(string accessToken, string playlistId, string[] trackUris)
    {
        SetAuthorizationHeader(accessToken);

        // Spotify API limit: 100 tracks per request
        const int batchSize = 100;
        
        for (int i = 0; i < trackUris.Length; i += batchSize)
        {
            var batch = trackUris.Skip(i).Take(batchSize).ToArray();
            var payload = new { uris = batch };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{SpotifyApiBaseUrl}/playlists/{playlistId}/tracks", content);
            response.EnsureSuccessStatusCode();
            
            _logger.LogInformation("Added {Count} tracks to playlist {PlaylistId} (batch {BatchNum})", 
                batch.Length, playlistId, (i / batchSize) + 1);
        }
    }

    public async Task<SpotifyPlaylist> GetPlaylistAsync(string accessToken, string playlistId)
    {
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

    public async Task<SpotifyPlaylistTrack[]> GetPlaylistTracksAsync(string accessToken, string playlistId)
    {
        SetAuthorizationHeader(accessToken);

        var allTracks = new List<SpotifyPlaylistTrack>();
        var url = $"{SpotifyApiBaseUrl}/playlists/{playlistId}/tracks?limit=100";

        do
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);
            var items = result.GetProperty("items");

            foreach (var item in items.EnumerateArray())
            {
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

    public async Task RemoveTracksFromPlaylistAsync(string accessToken, string playlistId, string[] trackUris)
    {
        SetAuthorizationHeader(accessToken);

        var tracks = trackUris.Select(uri => new { uri }).ToArray();
        var payload = new { tracks };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Delete, $"{SpotifyApiBaseUrl}/playlists/{playlistId}/tracks")
        {
            Content = content
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Removed {Count} tracks from playlist {PlaylistId}", trackUris.Length, playlistId);
    }

    public async Task<SpotifyPlaylist[]> GetUserPlaylistsAsync(string accessToken, int limit = 50)
    {
        SetAuthorizationHeader(accessToken);

        var allPlaylists = new List<SpotifyPlaylist>();
        var url = $"{SpotifyApiBaseUrl}/me/playlists?limit=50&offset=0";

        do
        {
            _logger.LogInformation("Fetching playlists from: {Url}", url);
            
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Spotify API response (first 500 chars): {Content}", 
                content.Length > 500 ? content.Substring(0, 500) : content);
            
            var result = JsonSerializer.Deserialize<JsonElement>(content);
            var items = result.GetProperty("items");
            
            var hasNext = result.TryGetProperty("next", out var next) && next.ValueKind != JsonValueKind.Null;
            var total = result.TryGetProperty("total", out var totalProp) ? totalProp.GetInt32() : -1;
            _logger.LogInformation("Spotify says total playlists: {Total}, has next page: {HasNext}", total, hasNext);

            var batchCount = 0;
            foreach (var item in items.EnumerateArray())
            {
                var playlistName = item.GetProperty("name").GetString()!;
                allPlaylists.Add(new SpotifyPlaylist(
                    item.GetProperty("id").GetString()!,
                    playlistName,
                    item.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                    item.GetProperty("uri").GetString()!,
                    item.GetProperty("tracks").GetProperty("total").GetInt32()
                ));
                batchCount++;
                _logger.LogInformation("Added playlist: {Name}", playlistName);
            }

            _logger.LogInformation("Retrieved {BatchCount} playlists in this batch, {TotalCount} total so far", 
                batchCount, allPlaylists.Count);

            url = hasNext ? next.GetString() : null;

        } while (url != null);

        _logger.LogInformation("Retrieved {Count} playlists for user", allPlaylists.Count);

        return allPlaylists.ToArray();
    }
}
