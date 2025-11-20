using API.DTOs.Spotify;

namespace API.Interfaces;

public interface IDiscoveryQueryGenerator {
    Task<List<string>> GenerateQueriesAsync(SpotifyTrack[] topSavedTracks);
    Task<List<string>> AdaptQueriesAsync(SpotifyTrack[] topSavedTracks, List<string> currentQueries, int currentCount, int targetCount, string userEmail);
}