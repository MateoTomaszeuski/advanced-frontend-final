using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using API.DTOs.Spotify;
using API.Interfaces;
using API.Services.Helpers;

namespace API.Services.Spotify;

public class SpotifyUserService : ISpotifyUserService {
    private readonly HttpClient _httpClient;
    private readonly ILogger<SpotifyUserService> _logger;
    private const string SpotifyApiBaseUrl = "https://api.spotify.com/v1";

    public SpotifyUserService(HttpClient httpClient, ILogger<SpotifyUserService> logger) {
        _httpClient = httpClient;
        _logger = logger;
    }

    private void SetAuthorizationHeader(string accessToken) {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public async Task<string> GetCurrentUserIdAsync(string accessToken) {
        SetAuthorizationHeader(accessToken);
        var response = await _httpClient.GetAsync($"{SpotifyApiBaseUrl}/me");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var userProfile = JsonSerializer.Deserialize<JsonElement>(content);
        return userProfile.GetProperty("id").GetString() ?? throw new Exception("Could not get user ID");
    }

    public async Task<SpotifyUserProfile> GetCurrentUserProfileAsync(string accessToken) {
        SetAuthorizationHeader(accessToken);
        var response = await _httpClient.GetAsync($"{SpotifyApiBaseUrl}/me");

        if (!response.IsSuccessStatusCode) {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Spotify profile request failed with status {StatusCode}. Token length: {TokenLength}. Error: {Error}",
                response.StatusCode, accessToken?.Length ?? 0, errorContent);
        }

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var userJson = JsonSerializer.Deserialize<JsonElement>(content);

        var id = userJson.GetProperty("id").GetString() ?? "";
        var displayName = userJson.GetProperty("display_name").GetString() ?? "";
        var email = userJson.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;
        var country = userJson.TryGetProperty("country", out var countryProp) ? countryProp.GetString() : null;

        string? imageUrl = null;
        if (userJson.TryGetProperty("images", out var images) && images.GetArrayLength() > 0) {
            imageUrl = images[0].GetProperty("url").GetString();
        }

        return new SpotifyUserProfile(id, displayName, email ?? "", country, imageUrl);
    }
}