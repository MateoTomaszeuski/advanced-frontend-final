using API.Models;

namespace API.Services;

public interface ISpotifyTokenService
{
    Task<string> GetValidAccessTokenAsync(User user);
    bool IsTokenExpired(User user);
}

public class SpotifyTokenService : ISpotifyTokenService
{
    private readonly ISpotifyService _spotifyService;
    private readonly IUserService _userService;
    private readonly ILogger<SpotifyTokenService> _logger;
    private const int TokenExpiryBufferMinutes = 5;

    public SpotifyTokenService(
        ISpotifyService spotifyService,
        IUserService userService,
        ILogger<SpotifyTokenService> logger)
    {
        _spotifyService = spotifyService;
        _userService = userService;
        _logger = logger;
    }

    public bool IsTokenExpired(User user)
    {
        if (!user.SpotifyTokenExpiry.HasValue)
        {
            return true;
        }

        return user.SpotifyTokenExpiry.Value.AddMinutes(-TokenExpiryBufferMinutes) <= DateTime.UtcNow;
    }

    public async Task<string> GetValidAccessTokenAsync(User user)
    {
        if (string.IsNullOrEmpty(user.SpotifyAccessToken))
        {
            throw new InvalidOperationException("Spotify account not connected");
        }

        if (!IsTokenExpired(user))
        {
            return user.SpotifyAccessToken;
        }

        if (string.IsNullOrEmpty(user.SpotifyRefreshToken))
        {
            throw new InvalidOperationException("Spotify refresh token not available. Please reconnect your Spotify account.");
        }

        _logger.LogInformation("Refreshing expired Spotify token for user: {Email}", user.Email);

        try
        {
            var newAccessToken = await _spotifyService.RefreshAccessTokenAsync(user.SpotifyRefreshToken);
            
            user.SpotifyAccessToken = newAccessToken;
            user.SpotifyTokenExpiry = DateTime.UtcNow.AddHours(1);
            
            await _userService.UpdateUserAsync(user);

            _logger.LogInformation("Successfully refreshed and updated Spotify token for user: {Email}", user.Email);
            
            return newAccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh Spotify token for user: {Email}", user.Email);
            throw new InvalidOperationException("Failed to refresh Spotify token. Please reconnect your Spotify account.", ex);
        }
    }
}
