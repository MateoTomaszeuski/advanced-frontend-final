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
