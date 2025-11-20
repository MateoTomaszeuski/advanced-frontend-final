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
- artist: 'artist:""Artist Name""'
- year: 'year:2020' or 'year:1980-1990'
- genre: 'genre:rock' 'genre:jazz' 'genre:electronic' 'genre:hip-hop' 'genre:pop' 'genre:indie' 'genre:metal' etc.

REASONING PROCESS:
1. Analyze the user's top tracks to identify their preferred genres, eras, and artists
2. Think about SIMILAR well-known artists they might not have discovered yet
3. Build SIMPLE queries focused on those artists or genre/year combinations

QUERY BUILDING:
- Focus on specific well-known artists similar to what they like
- Use simple genre + year combinations for era-based discovery
- Keep each query simple - 1 artist OR 1 genre+year combo
- Avoid complex queries with many keywords

Examples:
- User likes: 'Tame Impala, Mac DeMarco, MGMT'
  → Queries: 'artist:""Unknown Mortal Orchestra""', 'artist:""Beach House""', 'genre:indie year:2015-2024'
- User likes: 'Kendrick Lamar, J. Cole, Drake'
  → Queries: 'artist:""Tyler, The Creator""', 'artist:""Vince Staples""', 'genre:hip-hop year:2018-2024'

Generate 5-7 simple search queries based on the user's top tracks.

Return your response in the following JSON format:
{
  ""queries"": [""query1"", ""query2"", ""query3""]
}

IMPORTANT: Focus on well-known artists similar to their taste!"),
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

REASONING PROCESS:
1. Look at what the user likes and what queries were already tried
2. Think of OTHER well-known artists in similar genres/styles
3. Consider expanding time periods slightly
4. Keep queries SIMPLE - one artist or one genre+year combo per query

The current search queries aren't finding enough NEW tracks. Generate 5-7 DIFFERENT but SIMPLE search queries:
- Try different well-known artists from similar genres
- Expand year ranges slightly (e.g., 2020-2024 → 2018-2024)
- Use related genres with time periods
- Keep each query simple and focused

Examples:
- Already tried: 'artist:""Tame Impala""'
  → New: 'artist:""King Gizzard""', 'artist:""Pond""', 'genre:indie year:2010-2020'
- Already tried: 'genre:hip-hop year:2020-2024'
  → New: 'artist:""JID""', 'artist:""Denzel Curry""', 'genre:hip-hop year:2018-2023'

Return your response in this JSON format:
{
  ""queries"": [""query1"", ""query2"", ""query3""]
}

IMPORTANT: Keep it simple - famous artists work best!"),
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