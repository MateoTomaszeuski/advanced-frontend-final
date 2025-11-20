namespace API.Services.Helpers;

public static class PlaylistHelper {
    public static string ParsePromptToSearchQuery(string prompt) {
        var lowerPrompt = prompt.ToLower();

        if (lowerPrompt.Contains("workout") || lowerPrompt.Contains("gym") || lowerPrompt.Contains("exercise")) {
            return "genre:rock energy:0.7-1.0";
        } else if (lowerPrompt.Contains("chill") || lowerPrompt.Contains("relax") || lowerPrompt.Contains("calm")) {
            return "genre:indie energy:0.1-0.5";
        } else if (lowerPrompt.Contains("party") || lowerPrompt.Contains("dance") || lowerPrompt.Contains("upbeat")) {
            return "genre:pop genre:dance energy:0.7-1.0";
        } else if (lowerPrompt.Contains("focus") || lowerPrompt.Contains("study") || lowerPrompt.Contains("concentration")) {
            return "genre:classical genre:ambient instrumentalness:0.5-1.0";
        }

        return prompt;
    }

    public static string GeneratePlaylistName(string prompt) {
        var lowerPrompt = prompt.ToLower();

        if (lowerPrompt.Contains("workout") || lowerPrompt.Contains("gym"))
            return "AI Workout Mix";
        else if (lowerPrompt.Contains("chill") || lowerPrompt.Contains("relax"))
            return "AI Chill Vibes";
        else if (lowerPrompt.Contains("party") || lowerPrompt.Contains("dance"))
            return "AI Party Mix";
        else if (lowerPrompt.Contains("focus") || lowerPrompt.Contains("study"))
            return "AI Focus Zone";

        return $"AI Playlist - {DateTime.UtcNow:MMM dd}";
    }
}