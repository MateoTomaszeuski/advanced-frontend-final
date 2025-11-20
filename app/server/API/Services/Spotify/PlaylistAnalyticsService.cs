using API.DTOs.Spotify;
using API.Interfaces;

namespace API.Services.Spotify;

public class PlaylistAnalyticsService : IPlaylistAnalyticsService {
    private readonly ISpotifyService _spotifyService;
    private readonly ILogger<PlaylistAnalyticsService> _logger;

    public PlaylistAnalyticsService(
        ISpotifyService spotifyService,
        ILogger<PlaylistAnalyticsService> logger) {
        _spotifyService = spotifyService;
        _logger = logger;
    }

    public async Task<PlaylistAnalyticsResponse> GetPlaylistAnalyticsAsync(string accessToken, string playlistId) {
        var playlist = await _spotifyService.GetPlaylistAsync(accessToken, playlistId);
        var tracks = await _spotifyService.GetPlaylistTracksAsync(accessToken, playlistId);

        if (tracks.Length == 0) {
            throw new InvalidOperationException("Playlist has no tracks");
        }

        var trackIds = tracks.Select(t => t.Id).ToArray();

        try {
            var audioFeatures = await _spotifyService.GetAudioFeaturesAsync(accessToken, trackIds);

            if (audioFeatures.Length == 0) {
                throw new InvalidOperationException("No audio features returned");
            }

            var avgDanceability = audioFeatures.Average(af => af.Danceability);
            var avgEnergy = audioFeatures.Average(af => af.Energy);
            var avgValence = audioFeatures.Average(af => af.Valence);
            var avgAcousticness = audioFeatures.Average(af => af.Acousticness);
            var avgInstrumentalness = audioFeatures.Average(af => af.Instrumentalness);
            var avgLiveness = audioFeatures.Average(af => af.Liveness);
            var avgSpeechiness = audioFeatures.Average(af => af.Speechiness);
            var avgTempo = audioFeatures.Average(af => af.Tempo);
            var avgLoudness = audioFeatures.Average(af => af.Loudness);

            var minTempo = audioFeatures.Min(af => af.Tempo);
            var maxTempo = audioFeatures.Max(af => af.Tempo);
            var minEnergy = audioFeatures.Min(af => af.Energy);
            var maxEnergy = audioFeatures.Max(af => af.Energy);

            var keyDistribution = audioFeatures
                .GroupBy(af => af.Key)
                .ToDictionary(g => g.Key, g => g.Count());

            var modeDistribution = audioFeatures
                .GroupBy(af => af.Mode)
                .ToDictionary(g => g.Key, g => g.Count());

            return new PlaylistAnalyticsResponse(
                playlist.Id,
                playlist.Name,
                tracks.Length,
                new AudioFeaturesStats(
                    avgDanceability,
                    avgEnergy,
                    avgValence,
                    avgAcousticness,
                    avgInstrumentalness,
                    avgLiveness,
                    avgSpeechiness,
                    avgTempo,
                    avgLoudness,
                    minTempo,
                    maxTempo,
                    minEnergy,
                    maxEnergy
                ),
                new Dictionary<string, int>(),
                keyDistribution,
                modeDistribution,
                new Dictionary<string, int>()
            );
        } catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden) {
            _logger.LogWarning(ex, "Spotify Audio Features API returned 403 - using basic analytics only");
            throw new UnauthorizedAccessException(
                "Spotify Audio Features API access denied. Your Spotify Developer account needs extended quota approval.", ex);
        } catch (Exception ex) {
            _logger.LogWarning(ex, "Failed to fetch audio features - returning basic analytics");
            throw new Exception("Audio features temporarily unavailable", ex);
        }
    }
}