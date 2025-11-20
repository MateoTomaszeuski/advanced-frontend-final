using API.DTOs.Spotify;

namespace API.Interfaces;

public interface ITrackDiscoveryHelper {
    Task<List<SpotifyTrack>> DiscoverWithoutSavedTracksAsync(string accessToken, int limit, HashSet<string> savedTrackIds);
    Task<List<SpotifyTrack>> DiscoverFromSearchQueriesAsync(string accessToken, List<string> searchQueries, int limit, HashSet<string> savedTrackIds, string userEmail);
    Task<List<SpotifyTrack>> FallbackToRecommendationsAsync(string accessToken, SpotifyTrack[] seedTracks, int limit, List<SpotifyTrack> existingTracks, HashSet<string> savedTrackIds);
}