namespace API.Interfaces;

public interface ISpotifyAuthService {
    Task<string> RefreshAccessTokenAsync(string refreshToken);
}