using API.DTOs.Spotify;

namespace API.Interfaces;

public interface ISpotifyUserService {
    Task<string> GetCurrentUserIdAsync(string accessToken);
    Task<SpotifyUserProfile> GetCurrentUserProfileAsync(string accessToken);
}