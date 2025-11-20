using API.Models;

namespace API.Interfaces;

public interface ISpotifyTokenService {
    Task<string> GetValidAccessTokenAsync(User user);
    bool IsTokenExpired(User user);
}