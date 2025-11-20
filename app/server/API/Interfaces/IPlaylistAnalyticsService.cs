using API.DTOs.Spotify;

namespace API.Interfaces;

public interface IPlaylistAnalyticsService {
    Task<PlaylistAnalyticsResponse> GetPlaylistAnalyticsAsync(string accessToken, string playlistId);
}