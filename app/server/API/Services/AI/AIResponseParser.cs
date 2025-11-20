using System.Text.Json;
using API.Interfaces;
using API.Services.Helpers;

namespace API.Services.AI;

public class AIResponseParser : IAIResponseParser {
    private readonly ILogger<AIResponseParser> _logger;

    public AIResponseParser(ILogger<AIResponseParser> logger) {
        _logger = logger;
    }

    public (string playlistName, string searchQuery, string description) ParsePlaylistResponse(string? aiResponse, string fallbackPrompt) {
        if (string.IsNullOrEmpty(aiResponse)) {
            _logger.LogWarning("AI response is empty, using fallback");
            return (
                PlaylistHelper.GeneratePlaylistName(fallbackPrompt),
                PlaylistHelper.ParsePromptToSearchQuery(fallbackPrompt),
                $"AI-generated playlist: {fallbackPrompt}"
            );
        }

        try {
            var jsonStart = aiResponse.IndexOf('{');
            var jsonEnd = aiResponse.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd > jsonStart) {
                var jsonStr = aiResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var aiData = JsonSerializer.Deserialize<JsonElement>(jsonStr);
                var playlistName = aiData.GetProperty("playlistName").GetString() ?? PlaylistHelper.GeneratePlaylistName(fallbackPrompt);
                var searchQuery = aiData.GetProperty("searchQuery").GetString() ?? fallbackPrompt;
                var description = aiData.GetProperty("description").GetString() ?? $"AI-generated playlist: {fallbackPrompt}";
                return (playlistName, searchQuery, description);
            }
            throw new Exception("No JSON found in AI response");
        } catch (Exception ex) {
            _logger.LogWarning(ex, "Failed to parse AI response: {Response}", aiResponse);
            return (
                PlaylistHelper.GeneratePlaylistName(fallbackPrompt),
                PlaylistHelper.ParsePromptToSearchQuery(fallbackPrompt),
                $"AI-generated playlist: {fallbackPrompt}"
            );
        }
    }

    public string[] ParseQueriesResponse(string? aiResponse, string[]? fallback = null) {
        if (string.IsNullOrEmpty(aiResponse)) {
            return fallback ?? Array.Empty<string>();
        }

        try {
            var jsonStart = aiResponse.IndexOf('{');
            var jsonEnd = aiResponse.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd > jsonStart) {
                var jsonStr = aiResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var aiData = JsonSerializer.Deserialize<JsonElement>(jsonStr);
                var queries = aiData.GetProperty("queries").EnumerateArray()
                    .Select(q => q.GetString() ?? "")
                    .Where(q => !string.IsNullOrEmpty(q))
                    .ToArray();
                return queries.Length > 0 ? queries : (fallback ?? Array.Empty<string>());
            }
        } catch (Exception ex) {
            _logger.LogWarning(ex, "Failed to parse queries from AI response");
        }

        return fallback ?? Array.Empty<string>();
    }

    public string[] ParseGenresResponse(string? aiResponse) {
        if (string.IsNullOrEmpty(aiResponse)) {
            return new[] { "pop", "rock", "indie", "electronic", "jazz" };
        }

        try {
            var jsonStart = aiResponse.IndexOf('{');
            var jsonEnd = aiResponse.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd > jsonStart) {
                var jsonStr = aiResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var aiData = JsonSerializer.Deserialize<JsonElement>(jsonStr);
                var genres = aiData.GetProperty("genres").EnumerateArray()
                    .Select(g => g.GetString() ?? "pop")
                    .ToArray();
                return genres.Length > 0 ? genres : new[] { "pop", "rock", "indie", "electronic", "jazz" };
            }
        } catch (Exception ex) {
            _logger.LogWarning(ex, "Failed to parse genres from AI response");
        }

        return new[] { "pop", "rock", "indie", "electronic", "jazz" };
    }
}