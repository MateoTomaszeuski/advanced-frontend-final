using API.DTOs.Spotify;

namespace API.Services.Helpers;

public static class TrackDeduplicationHelper {
    public static string GetTrackKey(SpotifyTrack track) {
        var normalizedName = TrackNormalizationHelper.NormalizeTrackName(track.Name);
        var artistNames = string.Join(",", track.Artists.Select(a => a.Name.ToLowerInvariant()).OrderBy(n => n));
        return $"{normalizedName}|{artistNames}";
    }

    public static bool IsDuplicateTrack(
        SpotifyTrack track,
        IEnumerable<SpotifyTrack> existingTracks) {
        return existingTracks.Any(existingTrack =>
            TrackNormalizationHelper.AreTitlesSimilar(existingTrack.Name, track.Name) &&
            TrackNormalizationHelper.AreArtistsSimilar(
                existingTrack.Artists.Select(a => a.Name).ToArray(),
                track.Artists.Select(a => a.Name).ToArray()));
    }

    public static DateTime? ParseReleaseDate(string dateString) {
        if (DateTime.TryParse(dateString, out var date)) {
            return date;
        }
        return null;
    }
}