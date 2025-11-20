using API.DTOs.Agent;
using API.DTOs.Spotify;
using API.Interfaces;

namespace API.Services.Helpers;

public class TrackFilterHelper {
    private readonly ISpotifyService _spotifyService;

    public TrackFilterHelper(ISpotifyService spotifyService) {
        _spotifyService = spotifyService;
    }

    public async Task<SpotifyTrack[]> FilterByPreferencesAsync(
        string accessToken,
        SpotifyTrack[] tracks,
        PlaylistPreferences preferences) {
        if (preferences.MinEnergy == null && preferences.MaxEnergy == null &&
            preferences.MinTempo == null && preferences.MaxTempo == null) {
            return tracks;
        }

        var trackIds = tracks.Select(t => t.Id).ToArray();
        var audioFeatures = await _spotifyService.GetAudioFeaturesAsync(accessToken, trackIds);

        var filteredTracks = new List<SpotifyTrack>();

        for (int i = 0; i < tracks.Length && i < audioFeatures.Length; i++) {
            var track = tracks[i];
            var features = audioFeatures[i];

            if (MeetsPreferences(features, preferences)) {
                filteredTracks.Add(track);
            }
        }

        return filteredTracks.ToArray();
    }

    private static bool MeetsPreferences(AudioFeatures features, PlaylistPreferences preferences) {
        if (preferences.MinEnergy.HasValue && features.Energy * 100 < preferences.MinEnergy.Value)
            return false;

        if (preferences.MaxEnergy.HasValue && features.Energy * 100 > preferences.MaxEnergy.Value)
            return false;

        if (preferences.MinTempo.HasValue && features.Tempo < preferences.MinTempo.Value)
            return false;

        if (preferences.MaxTempo.HasValue && features.Tempo > preferences.MaxTempo.Value)
            return false;

        return true;
    }
}