namespace API.Services.Helpers;

public static class TrackNormalizationHelper {
    public static string NormalizeTrackName(string name) {
        var normalized = name.ToLowerInvariant();
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\s*\(.*?\)\s*", " ");
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\s*\[.*?\]\s*", " ");
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\b(the|a|an|my|your|our)\b", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"[^\w\s]", " ");
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\s+", " ");
        return normalized.Trim();
    }

    public static bool AreTitlesSimilar(string title1, string title2) {
        var normalized1 = NormalizeTrackName(title1);
        var normalized2 = NormalizeTrackName(title2);

        if (normalized1 == normalized2) return true;

        var words1 = normalized1.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var words2 = normalized2.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (words1.Length > 0 && words2.Length > 0) {
            var commonWords = words1.Intersect(words2).Count();
            var similarity = (double)commonWords / Math.Min(words1.Length, words2.Length);
            return similarity > 0.6;
        }

        return false;
    }

    public static bool AreArtistsSimilar(string[] artists1, string[] artists2) {
        var set1 = artists1.Select(a => a.ToLowerInvariant()).ToHashSet();
        var set2 = artists2.Select(a => a.ToLowerInvariant()).ToHashSet();
        return set1.Overlaps(set2) || set1.SetEquals(set2);
    }
}