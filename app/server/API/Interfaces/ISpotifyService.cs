using API.DTOs.Spotify;

namespace API.Interfaces;

public interface ISpotifyService {
    Task<string> RefreshAccessTokenAsync(string refreshToken);
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