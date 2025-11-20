using System.Text.Json;
using API.DTOs.Spotify;
using API.Interfaces;
using API.Models.AI;

namespace API.Services.Helpers;

public class DiscoveryQueryGenerator : IDiscoveryQueryGenerator {
    private readonly IAIService _aiService;
    private readonly IAgentNotificationService _notificationService;
    private readonly ILogger<DiscoveryQueryGenerator> _logger;

    public DiscoveryQueryGenerator(
        IAIService aiService,
        IAgentNotificationService notificationService,
        ILogger<DiscoveryQueryGenerator> logger) {
        _aiService = aiService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<List<string>> GenerateQueriesAsync(SpotifyTrack[] topSavedTracks) {
        var aiMessages = new List<AIMessage>
        {
            new AIMessage("system", @"You are a music expert assistant. Analyze the user's favorite tracks and generate search queries to discover similar but new music.

SPOTIFY SEARCH API - OFFICIAL SUPPORTED FILTERS:
- album: 'album:""Album Name""'
- artist: 'artist:""Artist Name""'
- track: 'track:""Track Name""'
- year: 'year:2020' or 'year:1980-1990'
- genre: 'genre:rock' 'genre:jazz' 'genre:electronic' 'genre:hip-hop' 'genre:pop' 'genre:indie' 'genre:metal' etc.

QUERY BUILDING:
1. Mix keywords with filters for best results
2. Use artist filters to find similar artists: 'artist:""Similar Artist""'
3. Use genre filters to explore genres: 'genre:genrename'
4. Combine multiple filters: 'genre:indie year:2020-2024'

Examples:
- 'artist:""Arctic Monkeys"" genre:indie genre:rock'
- 'chill lofi genre:hip-hop genre:electronic'
- 'upbeat genre:pop year:2020-2024'

Generate 3-5 diverse search queries based on the genres and artists from the user's top tracks.

Return your response in the following JSON format:
{
  ""queries"": [""query1"", ""query2"", ""query3""]
}

IMPORTANT: Generate diverse, unique queries - do not repeat previous suggestions."),
            new AIMessage("user", $"[Request #{DateTime.UtcNow.Ticks}] User's top tracks: {string.Join(", ", topSavedTracks.Select(t => $"{t.Name} by {string.Join(", ", t.Artists.Select(a => a.Name))}"))}. Generate search queries to discover similar music.")
        };

        var aiResponse = await _aiService.GetChatCompletionAsync(aiMessages);
        var searchQueries = new List<string>();

        if (!string.IsNullOrEmpty(aiResponse.Response)) {
            try {
                var jsonStart = aiResponse.Response.IndexOf('{');
                var jsonEnd = aiResponse.Response.LastIndexOf('}');
                if (jsonStart >= 0 && jsonEnd > jsonStart) {
                    var jsonStr = aiResponse.Response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    var aiData = JsonSerializer.Deserialize<JsonElement>(jsonStr);
                    var queryArray = aiData.GetProperty("queries");
                    searchQueries = queryArray.EnumerateArray()
                        .Select(q => q.GetString() ?? "")
                        .Where(q => !string.IsNullOrEmpty(q))
                        .ToList();
                }
            } catch (Exception ex) {
                _logger.LogWarning(ex, "Failed to parse AI queries, using fallback");
            }
        }

        if (searchQueries.Count == 0) {
            var topArtists = topSavedTracks
                .SelectMany(t => t.Artists)
                .GroupBy(a => a.Name)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => g.Key)
                .ToArray();

            searchQueries.Add(string.Join(" ", topArtists));
            searchQueries.Add("recommendations");
        }

        return searchQueries;
    }

    public async Task<List<string>> AdaptQueriesAsync(
        SpotifyTrack[] topSavedTracks,
        List<string> currentQueries,
        int currentCount,
        int targetCount,
        string userEmail) {
        _logger.LogInformation("Still need {Missing} more tracks. Asking AI for adapted queries...",
            targetCount - currentCount);

        var aiMessages = new List<AIMessage>
        {
            new AIMessage("system", @"You are a music expert helping discover new music for a user.

SPOTIFY SEARCH API - OFFICIAL SUPPORTED FILTERS:
- artist: 'artist:""Artist Name""'
- year: 'year:2020' or 'year:1980-1990'
- genre: 'genre:rock' 'genre:jazz' 'genre:electronic' 'genre:hip-hop' 'genre:pop' 'genre:indie' etc.

The current search queries aren't finding enough NEW tracks (that the user hasn't saved). Generate 3-5 DIFFERENT search queries that:
1. Explore similar but different artists
2. Try related or adjacent genres
3. Search for newer or older tracks in the same style
4. Use broader or more creative keywords

Return your response in this JSON format:
{
  ""queries"": [""query1"", ""query2"", ""query3""]
}

IMPORTANT: Generate diverse alternatives - these must be tracks the user likely hasn't heard!"),
            new AIMessage("user", $"[Request #{DateTime.UtcNow.Ticks}] User's top tracks: {string.Join(", ", topSavedTracks.Take(3).Select(t => $"{t.Name} by {string.Join(", ", t.Artists.Select(a => a.Name))}"))}.\nCurrent queries: {string.Join(", ", currentQueries)}\nFound {currentCount} new tracks so far, need {targetCount} total.\n\nGenerate alternative search queries to discover more new music.")
        };

        try {
            var aiResponse = await _aiService.GetChatCompletionAsync(aiMessages);

            if (!string.IsNullOrEmpty(aiResponse.Response)) {
                var jsonStart = aiResponse.Response.IndexOf('{');
                var jsonEnd = aiResponse.Response.LastIndexOf('}');
                if (jsonStart >= 0 && jsonEnd > jsonStart) {
                    var jsonStr = aiResponse.Response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    var aiData = JsonSerializer.Deserialize<JsonElement>(jsonStr);
                    var newQueries = aiData.GetProperty("queries").EnumerateArray()
                        .Select(q => q.GetString() ?? "")
                        .Where(q => !string.IsNullOrEmpty(q))
                        .ToList();

                    if (newQueries.Any()) {
                        _logger.LogInformation("AI generated {Count} new discovery queries: {Queries}",
                            newQueries.Count, string.Join(", ", newQueries));
                        await _notificationService.SendStatusUpdateAsync(userEmail, "processing",
                            $"AI generated {newQueries.Count} new discovery strategies to find more tracks");
                        return newQueries;
                    }
                }
            }
        } catch (Exception ex) {
            _logger.LogWarning(ex, "Failed to get AI adaptation for discovery, continuing with existing queries");
        }

        return currentQueries;
    }
}