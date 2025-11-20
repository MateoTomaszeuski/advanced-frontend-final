using API.DTOs.Spotify;

namespace API.Interfaces;

public interface ISpotifyTrackService {
    Task<SpotifyTrack[]> SearchTracksAsync(string accessToken, string query, int limit = 20);
    Task<SpotifyTrack[]> GetRecommendationsAsync(string accessToken, string[] seedTracks, int limit = 20);
    Task<AudioFeatures[]> GetAudioFeaturesAsync(string accessToken, string[] trackIds);
    Task<SpotifyTrack[]> GetUserSavedTracksAsync(string accessToken, int limit = 50);
    Task<SpotifyPlaylistTrack[]> GetPlaylistTracksAsync(string accessToken, string playlistId);
}