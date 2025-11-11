using API.Extensions;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SpotifyController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ISpotifyService _spotifyService;
    private readonly ISpotifyTokenService _tokenService;
    private readonly ILogger<SpotifyController> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public SpotifyController(
        IUserService userService,
        ISpotifyService spotifyService,
        ISpotifyTokenService tokenService,
        ILogger<SpotifyController> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _userService = userService;
        _spotifyService = spotifyService;
        _tokenService = tokenService;
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
    }

    [HttpPost("exchange-code")]
    public async Task<IActionResult> ExchangeAuthorizationCode([FromBody] ExchangeCodeRequest request)
    {
        var user = this.GetCurrentUser();
        if (user == null)
        {
            return this.UnauthorizedUser();
        }

        _logger.LogInformation("Exchanging Spotify authorization code for user: {Email}", user.Email);

        try
        {
            var clientId = _configuration["Spotify:ClientId"];
            var clientSecret = _configuration["Spotify:ClientSecret"];
            var configuredRedirectUri = _configuration["Spotify:RedirectUri"];
            var redirectUri = string.IsNullOrEmpty(configuredRedirectUri) ? request.RedirectUri : configuredRedirectUri;

            _logger.LogInformation("Using redirect URI: {RedirectUri} (configured: {ConfiguredRedirectUri}, requested: {RequestedRedirectUri})", 
                redirectUri, configuredRedirectUri, request.RedirectUri);

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                return BadRequest(new { error = "Spotify client credentials not configured" });
            }

            var tokenEndpoint = "https://accounts.spotify.com/api/token";
            var requestBody = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", request.Code },
                { "redirect_uri", redirectUri },
                { "client_id", clientId },
                { "client_secret", clientSecret }
            };

            var content = new FormUrlEncodedContent(requestBody);
            var response = await _httpClient.PostAsync(tokenEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Spotify token exchange failed with status {StatusCode}. RedirectUri used: {RedirectUri}. Error: {Error}", 
                    response.StatusCode, redirectUri, errorContent);
                return BadRequest(new { error = "Failed to exchange authorization code", details = errorContent });
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<SpotifyTokenResponse>(responseContent);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.access_token))
            {
                return BadRequest(new { error = "Invalid token response from Spotify" });
            }

            user.SpotifyAccessToken = tokenResponse.access_token;
            user.SpotifyRefreshToken = tokenResponse.refresh_token;
            user.SpotifyTokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in);

            await _userService.UpdateUserAsync(user);

            return Ok(new { message = "Spotify account connected successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging Spotify authorization code");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("connect")]
    public async Task<IActionResult> ConnectSpotifyAccount([FromBody] ConnectSpotifyRequest request)
    {
        var user = this.GetCurrentUser();
        if (user == null)
        {
            return this.UnauthorizedUser();
        }

        _logger.LogInformation("Connecting Spotify account for user: {Email}", user.Email);

        user.SpotifyAccessToken = request.AccessToken;
        user.SpotifyRefreshToken = request.RefreshToken;
        user.SpotifyTokenExpiry = DateTime.UtcNow.AddSeconds(request.ExpiresIn);

        await _userService.UpdateUserAsync(user);

        return Ok(new { message = "Spotify account connected successfully" });
    }

    [HttpGet("status")]
    public IActionResult GetConnectionStatus()
    {
        var user = this.GetCurrentUser();
        if (user == null)
        {
            return this.UnauthorizedUser();
        }

        _logger.LogInformation("Checking Spotify connection status for user: {Email}", user.Email);

        var isConnected = !string.IsNullOrEmpty(user.SpotifyAccessToken);
        var isTokenValid = user.SpotifyTokenExpiry.HasValue && user.SpotifyTokenExpiry.Value > DateTime.UtcNow;

        return Ok(new
        {
            isConnected = isConnected,
            isTokenValid = isTokenValid,
            tokenExpiry = user.SpotifyTokenExpiry
        });
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetSpotifyProfile()
    {
        var user = this.GetCurrentUser();
        if (user == null)
        {
            return this.UnauthorizedUser();
        }

        try
        {
            var accessToken = await _tokenService.GetValidAccessTokenAsync(user);
            var profile = await _spotifyService.GetCurrentUserProfileAsync(accessToken);
            return Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Spotify token issue for user: {Email}", user.Email);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Spotify profile for user: {Email}", user.Email);
            return StatusCode(500, new { error = "Failed to fetch Spotify profile" });
        }
    }

    [HttpPost("disconnect")]
    public async Task<IActionResult> DisconnectSpotifyAccount()
    {
        var user = this.GetCurrentUser();
        if (user == null)
        {
            return this.UnauthorizedUser();
        }

        _logger.LogInformation("Disconnecting Spotify account for user: {Email}", user.Email);

        user.SpotifyAccessToken = null;
        user.SpotifyRefreshToken = null;
        user.SpotifyTokenExpiry = null;

        await _userService.UpdateUserAsync(user);

        return Ok(new { message = "Spotify account disconnected successfully" });
    }

    [HttpGet("playlists")]
    public async Task<IActionResult> GetUserPlaylists()
    {
        var user = this.GetCurrentUser();
        if (user == null)
        {
            return this.UnauthorizedUser();
        }

        try
        {
            var accessToken = await _tokenService.GetValidAccessTokenAsync(user);
            var playlists = await _spotifyService.GetUserPlaylistsAsync(accessToken);
            return Ok(playlists);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Spotify token issue for user: {Email}", user.Email);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user playlists for user: {Email}", user.Email);
            return StatusCode(500, new { error = "Failed to fetch playlists" });
        }
    }

    [HttpPost("playlists/{playlistId}/tracks")]
    public async Task<IActionResult> AddTracksToPlaylist(string playlistId, [FromBody] AddTracksRequest request)
    {
        var user = this.GetCurrentUser();
        if (user == null)
        {
            return this.UnauthorizedUser();
        }

        try
        {
            var accessToken = await _tokenService.GetValidAccessTokenAsync(user);
            await _spotifyService.AddTracksToPlaylistAsync(accessToken, playlistId, request.TrackUris);
            return Ok(new { message = $"Successfully added {request.TrackUris.Length} tracks to playlist" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Spotify token issue for user: {Email}", user.Email);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding tracks to playlist {PlaylistId} for user: {Email}", playlistId, user.Email);
            return StatusCode(500, new { error = "Failed to add tracks to playlist" });
        }
    }

    [HttpGet("playlists/{playlistId}/analytics")]
    public async Task<IActionResult> GetPlaylistAnalytics(string playlistId)
    {
        var user = this.GetCurrentUser();
        if (user == null)
        {
            return this.UnauthorizedUser();
        }

        try
        {
            var accessToken = await _tokenService.GetValidAccessTokenAsync(user);
            
            // Get playlist details
            var playlist = await _spotifyService.GetPlaylistAsync(accessToken, playlistId);
            var tracks = await _spotifyService.GetPlaylistTracksAsync(accessToken, playlistId);
            
            if (tracks.Length == 0)
            {
                return BadRequest(new { error = "Playlist has no tracks" });
            }

            // Get audio features for all tracks - with graceful degradation
            var trackIds = tracks.Select(t => t.Id).ToArray();
            
            try
            {
                var audioFeatures = await _spotifyService.GetAudioFeaturesAsync(accessToken, trackIds);

                if (audioFeatures.Length == 0)
                {
                    throw new InvalidOperationException("No audio features returned");
                }

                // Calculate aggregate statistics
                var avgDanceability = audioFeatures.Average(af => af.Danceability);
                var avgEnergy = audioFeatures.Average(af => af.Energy);
                var avgValence = audioFeatures.Average(af => af.Valence);
                var avgAcousticness = audioFeatures.Average(af => af.Acousticness);
                var avgInstrumentalness = audioFeatures.Average(af => af.Instrumentalness);
                var avgLiveness = audioFeatures.Average(af => af.Liveness);
                var avgSpeechiness = audioFeatures.Average(af => af.Speechiness);
                var avgTempo = audioFeatures.Average(af => af.Tempo);
                var avgLoudness = audioFeatures.Average(af => af.Loudness);
                
                var minTempo = audioFeatures.Min(af => af.Tempo);
                var maxTempo = audioFeatures.Max(af => af.Tempo);
                var minEnergy = audioFeatures.Min(af => af.Energy);
                var maxEnergy = audioFeatures.Max(af => af.Energy);

                // Key distribution
                var keyDistribution = audioFeatures
                    .GroupBy(af => af.Key)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Mode distribution
                var modeDistribution = audioFeatures
                    .GroupBy(af => af.Mode)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Genre distribution (simplified - would need artist data for full genre info)
                var genres = new Dictionary<string, int>();

                // Decade distribution based on album release dates
                var decadeDistribution = new Dictionary<string, int>();

                var analytics = new API.DTOs.Spotify.PlaylistAnalyticsResponse(
                    playlist.Id,
                    playlist.Name,
                    tracks.Length,
                    new API.DTOs.Spotify.AudioFeaturesStats(
                        avgDanceability,
                        avgEnergy,
                        avgValence,
                        avgAcousticness,
                        avgInstrumentalness,
                        avgLiveness,
                        avgSpeechiness,
                        avgTempo,
                        avgLoudness,
                        minTempo,
                        maxTempo,
                        minEnergy,
                        maxEnergy
                    ),
                    genres,
                    keyDistribution,
                    modeDistribution,
                    decadeDistribution
                );

                return Ok(analytics);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                _logger.LogWarning(ex, "Spotify Audio Features API returned 403 - using basic analytics only");
                
                // Return basic analytics without audio features
                var basicAnalytics = new API.DTOs.Spotify.PlaylistAnalyticsResponse(
                    playlist.Id,
                    playlist.Name,
                    tracks.Length,
                    new API.DTOs.Spotify.AudioFeaturesStats(
                        0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, // Default values for audio features
                        120.0f, -5.0f, // Default tempo and loudness
                        60.0f, 180.0f, // Default min/max tempo
                        0.0f, 1.0f // Default min/max energy
                    ),
                    new Dictionary<string, int>(), // Empty genres
                    new Dictionary<int, int>(), // Empty key distribution
                    new Dictionary<int, int>(), // Empty mode distribution
                    new Dictionary<string, int>() // Empty decade distribution
                );
                
                return StatusCode(403, new { 
                    error = "Spotify Audio Features API access denied",
                    message = "Your Spotify Developer account needs extended quota approval to access audio features. The Audio Features API is restricted for new developer apps.",
                    details = "To enable full analytics: 1) Visit the Spotify Developer Dashboard, 2) Select your app, 3) Request a quota extension for the Audio Features API, 4) Explain your use case (educational project).",
                    fallback = basicAnalytics
                });
            }
            catch (Exception innerEx)
            {
                _logger.LogWarning(innerEx, "Failed to fetch audio features - returning basic analytics");
                
                // Return basic analytics without audio features
                var basicAnalytics = new API.DTOs.Spotify.PlaylistAnalyticsResponse(
                    playlist.Id,
                    playlist.Name,
                    tracks.Length,
                    new API.DTOs.Spotify.AudioFeaturesStats(
                        0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f,
                        120.0f, -5.0f,
                        60.0f, 180.0f,
                        0.0f, 1.0f
                    ),
                    new Dictionary<string, int>(),
                    new Dictionary<int, int>(),
                    new Dictionary<int, int>(),
                    new Dictionary<string, int>()
                );
                
                return StatusCode(503, new { 
                    error = "Audio features temporarily unavailable",
                    message = "Unable to fetch audio features at this time. Showing basic playlist information only.",
                    fallback = basicAnalytics
                });
            }
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Spotify token issue for user: {Email}", user.Email);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching playlist analytics for playlist {PlaylistId} for user: {Email}", playlistId, user.Email);
            return StatusCode(500, new { error = "Failed to fetch playlist analytics" });
        }
    }
}

public record ConnectSpotifyRequest(
    string AccessToken,
    string? RefreshToken,
    int ExpiresIn
);

public record ExchangeCodeRequest(
    string Code,
    string RedirectUri
);

public record SpotifyTokenResponse(
    string access_token,
    string token_type,
    int expires_in,
    string? refresh_token,
    string scope
);

public record AddTracksRequest(
    string[] TrackUris
);
