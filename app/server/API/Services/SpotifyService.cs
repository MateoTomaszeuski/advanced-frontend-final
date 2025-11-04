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

        var payload = new { uris = trackUris };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{SpotifyApiBaseUrl}/playlists/{playlistId}/tracks", content);
        response.EnsureSuccessStatusCode();
    }
}
