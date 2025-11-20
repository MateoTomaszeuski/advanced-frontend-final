using System.Text.Json;
using API.Interfaces;

namespace API.Services.Spotify;

public class SpotifyAuthService : ISpotifyAuthService {
    private readonly HttpClient _httpClient;
    private readonly ILogger<SpotifyAuthService> _logger;
    private readonly IConfiguration _configuration;

    public SpotifyAuthService(
        HttpClient httpClient,
        ILogger<SpotifyAuthService> logger,
        IConfiguration configuration) {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<string> RefreshAccessTokenAsync(string refreshToken) {
        try {
            var clientId = _configuration["Spotify:ClientId"];
            var clientSecret = _configuration["Spotify:ClientSecret"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret)) {
                throw new InvalidOperationException("Spotify client credentials not configured");
            }

            var tokenEndpoint = "https://accounts.spotify.com/api/token";
            var requestBody = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken },
                { "client_id", clientId },
                { "client_secret", clientSecret }
            };

            var content = new FormUrlEncodedContent(requestBody);
            var response = await _httpClient.PostAsync(tokenEndpoint, content);

            if (!response.IsSuccessStatusCode) {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Spotify token refresh failed with status {StatusCode}. Error: {Error}",
                    response.StatusCode, errorContent);
                throw new InvalidOperationException($"Failed to refresh Spotify token: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var newAccessToken = tokenResponse.GetProperty("access_token").GetString();
            if (string.IsNullOrEmpty(newAccessToken)) {
                throw new InvalidOperationException("Invalid token response from Spotify");
            }

            _logger.LogInformation("Successfully refreshed Spotify access token");
            return newAccessToken;
        } catch (Exception ex) {
            _logger.LogError(ex, "Error refreshing Spotify access token");
            throw;
        }
    }
}