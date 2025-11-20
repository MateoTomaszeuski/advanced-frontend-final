using API.DTOs.Spotify;

namespace API.Interfaces;

public interface ISpotifyPlaylistService {
    Task<SpotifyPlaylist> CreatePlaylistAsync(string accessToken, string userId, CreatePlaylistRequest request);
    Task<SpotifyPlaylist> GetPlaylistAsync(string accessToken, string playlistId);
    Task<SpotifyPlaylist[]> GetUserPlaylistsAsync(string accessToken, int limit = 50);
    Task AddTracksToPlaylistAsync(string accessToken, string playlistId, string[] trackUris);
    Task RemoveTracksFromPlaylistAsync(string accessToken, string playlistId, string[] trackUris);
}