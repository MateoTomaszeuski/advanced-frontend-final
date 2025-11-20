using System.Text.Json;
using API.DTOs.Agent;
using API.DTOs.Spotify;
using API.Interfaces;
using API.Models;
using API.Models.AI;

namespace API.Services.Agents;

public class MusicSuggestionService : IMusicSuggestionService {
    private readonly ISpotifyService _spotifyService;
    private readonly ISpotifyPlaylistService _playlistService;
    private readonly ISpotifyTrackService _trackService;
    private readonly ISpotifyTokenService _tokenService;
    private readonly IAIService _aiService;
    private readonly IAgentNotificationService _notificationService;
    private readonly ILogger<MusicSuggestionService> _logger;

    public MusicSuggestionService(
        ISpotifyService spotifyService,
        ISpotifyPlaylistService playlistService,
        ISpotifyTrackService trackService,
        ISpotifyTokenService tokenService,
        IAIService aiService,
        IAgentNotificationService notificationService,
        ILogger<MusicSuggestionService> logger) {
        _spotifyService = spotifyService;
        _playlistService = playlistService;
        _trackService = trackService;
        _tokenService = tokenService;
        _aiService = aiService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<SuggestMusicResponse> SuggestMusicByContextAsync(
        User user,
        SuggestMusicRequest request,
        int conversationId) {
        var accessToken = await _tokenService.GetValidAccessTokenAsync(user);

        _logger.LogInformation("Generating music suggestions for playlist {PlaylistId} with context: {Context}",
            request.PlaylistId, request.Context);

        var playlist = await _playlistService.GetPlaylistAsync(accessToken, request.PlaylistId);
        var playlistTracks = await _trackService.GetPlaylistTracksAsync(accessToken, request.PlaylistId);

        var topTracks = playlistTracks.OrderByDescending(t => t.Popularity).Take(10).ToArray();

        var (searchQueries, explanation) = await GenerateSuggestionQueriesAsync(playlist, topTracks, request.Context);

        var allSuggestions = new List<SuggestedTrack>();
        var existingTrackIds = playlistTracks.Select(t => t.Id).ToHashSet();
        var suggestionIds = new HashSet<string>();
        var currentSearchQueries = searchQueries.ToList();
        var searchIterations = 0;
        const int maxSearchIterations = 3;
        var targetCount = Math.Min(request.Limit, 50);

        while (allSuggestions.Count < targetCount && searchIterations < maxSearchIterations) {
            searchIterations++;

            foreach (var query in currentSearchQueries.ToList()) {
                if (allSuggestions.Count >= targetCount) break;

                _logger.LogInformation("Searching suggestions (iteration {Iteration}) with query: '{Query}'", searchIterations, query);
                await _notificationService.SendStatusUpdateAsync(user.Email, "processing", $"Suggestions iteration {searchIterations}: Searching with query '{query}'");

                var searchResults = await _spotifyService.SearchTracksAsync(accessToken, query, 50);

                var tracksFoundInThisQuery = 0;
                foreach (var track in searchResults) {
                    if (!existingTrackIds.Contains(track.Id) && suggestionIds.Add(track.Id)) {
                        allSuggestions.Add(new SuggestedTrack(
                            track.Id,
                            track.Name,
                            track.Artists.Select(a => a.Name).ToArray(),
                            track.Uri,
                            $"Matches '{query}' - {explanation}",
                            track.Popularity
                        ));
                        tracksFoundInThisQuery++;

                        if (allSuggestions.Count >= targetCount) break;
                    }
                }

                _logger.LogInformation("After query '{Query}': found {NewTracks} new suggestions, total {Count}",
                    query, tracksFoundInThisQuery, allSuggestions.Count);
                await _notificationService.SendStatusUpdateAsync(user.Email, "processing", $"After query '{query}': found {tracksFoundInThisQuery} new suggestions, total {allSuggestions.Count}");
            }

            if (allSuggestions.Count < targetCount && searchIterations < maxSearchIterations) {
                currentSearchQueries = await AdaptSuggestionQueriesAsync(playlist, topTracks, request.Context, currentSearchQueries, allSuggestions.Count, targetCount, user.Email);
            }
        }

        _logger.LogInformation("Generated {Count} suggestions for playlist {PlaylistId}",
            allSuggestions.Count, request.PlaylistId);

        return new SuggestMusicResponse(
            playlist.Id,
            playlist.Name,
            request.Context,
            allSuggestions.Count,
            allSuggestions.ToArray()
        );
    }

    private async Task<(List<string> queries, string explanation)> GenerateSuggestionQueriesAsync(SpotifyPlaylist playlist, SpotifyPlaylistTrack[] topTracks, string context) {
        var trackSummary = string.Join(", ", topTracks.Select(t =>
            $"{t.Name} by {string.Join(", ", t.Artists.Select(a => a.Name))}"));

        var aiMessages = new List<AIMessage>
        {
            new AIMessage("system", @"You are a music expert assistant. Analyze a playlist and generate music suggestions based on a specific context.

SPOTIFY SEARCH API - OFFICIAL SUPPORTED FILTERS:
- album: 'album:""Album Name""'
- artist: 'artist:""Artist Name""'
- track: 'track:""Track Name""'
- year: 'year:2020' or 'year:1980-1990'
- genre: 'genre:rock' 'genre:jazz' 'genre:electronic' 'genre:hip-hop' 'genre:pop' 'genre:indie' 'genre:metal' 'genre:country' 'genre:classical' 'genre:reggae' 'genre:blues' 'genre:soul' 'genre:funk' 'genre:punk' 'genre:folk' 'genre:r-n-b' 'genre:dance' 'genre:latin' 'genre:afrobeat'

QUERY BUILDING STRATEGY:
1. Analyze the playlist's style (genres, mood, era)
2. Match the user's context (e.g., 'workout', 'party', 'study', 'chill')
3. Combine keywords with filters for best results
4. Use artist filters to find similar artists
5. Use genre filters to explore related genres
6. Add year filters if context suggests a time period

Examples:
- Context 'workout' + indie rock playlist → 'energetic upbeat genre:rock genre:indie'
- Context 'party' + electronic playlist → 'dance party genre:electronic genre:dance year:2020-2024'
- Context 'study' + jazz playlist → 'calm focus genre:jazz genre:classical genre:ambient'

Generate 3-5 search queries that would find music matching the context while being similar to the playlist's style.

Return your response in the following JSON format:
{
  ""queries"": [""query1 with filters"", ""query2 with filters""],
  ""explanation"": ""Brief explanation of the suggestion strategy""
}

IMPORTANT: Generate diverse, creative queries - each request should yield unique suggestions."),
            new AIMessage("user", $"[Request #{DateTime.UtcNow.Ticks}] Playlist: {playlist.Name}\nTop tracks: {trackSummary}\nContext: {context}\n\nGenerate search queries to find songs that match this context while fitting the playlist's style.")
        };

        var aiResponse = await _aiService.GetChatCompletionAsync(aiMessages);

        var searchQueries = new List<string>();
        var explanation = "AI-generated suggestions based on playlist analysis";

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

                    if (aiData.TryGetProperty("explanation", out var exp)) {
                        explanation = exp.GetString() ?? explanation;
                    }
                }
            } catch (Exception ex) {
                _logger.LogWarning(ex, "Failed to parse AI response, using fallback");
            }
        }

        if (searchQueries.Count == 0) {
            var topArtists = topTracks
                .SelectMany(t => t.Artists)
                .GroupBy(a => a.Name)
                .OrderByDescending(g => g.Count())
                .Take(2)
                .Select(g => g.Key)
                .ToArray();

            searchQueries.Add(string.Join(" ", topArtists.Concat(new[] { context })));
        }

        return (searchQueries, explanation);
    }

    private async Task<List<string>> AdaptSuggestionQueriesAsync(SpotifyPlaylist playlist, SpotifyPlaylistTrack[] topTracks, string context, List<string> currentSearchQueries, int currentCount, int targetCount, string userEmail) {
        _logger.LogInformation("Still need {Missing} more suggestions. Asking AI for adapted queries...",
            targetCount - currentCount);

        var aiAdaptMessages = new List<AIMessage>
        {
            new AIMessage("system", @"You are a music expert providing contextual suggestions for a playlist.

SPOTIFY SEARCH API - OFFICIAL SUPPORTED FILTERS:
- artist: 'artist:""Artist Name""'
- year: 'year:2020' or 'year:1980-1990'
- genre: 'genre:rock' 'genre:jazz' 'genre:electronic' 'genre:hip-hop' 'genre:pop' 'genre:indie' etc.

The current queries aren't finding enough NEW tracks (not already in the playlist). Generate 3-5 DIFFERENT search queries that:
1. Better match the user's context/mood request
2. Explore different artists in a similar style
3. Try related sub-genres or crossover styles
4. Use more creative or specific keywords

Return your response in this JSON format:
{
  ""queries"": [""query1"", ""query2"", ""query3""]
}

IMPORTANT: Generate creative alternatives that match the context!"),
            new AIMessage("user", $"[Request #{DateTime.UtcNow.Ticks}] Playlist: {playlist.Name}\nContext requested: {context}\nTop tracks: {string.Join(", ", topTracks.Take(3).Select(t => $"{t.Name} by {string.Join(", ", t.Artists.Select(a => a.Name))}"))}.\nCurrent queries: {string.Join(", ", currentSearchQueries)}\nFound {currentCount} suggestions so far, need {targetCount} total.\n\nGenerate alternative search queries for better contextual suggestions.")
        };

        try {
            var aiAdaptResponse = await _aiService.GetChatCompletionAsync(aiAdaptMessages);

            if (!string.IsNullOrEmpty(aiAdaptResponse.Response)) {
                var jsonStart = aiAdaptResponse.Response.IndexOf('{');
                var jsonEnd = aiAdaptResponse.Response.LastIndexOf('}');
                if (jsonStart >= 0 && jsonEnd > jsonStart) {
                    var jsonStr = aiAdaptResponse.Response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    var aiData = JsonSerializer.Deserialize<JsonElement>(jsonStr);
                    var newQueries = aiData.GetProperty("queries").EnumerateArray()
                        .Select(q => q.GetString() ?? "")
                        .Where(q => !string.IsNullOrEmpty(q))
                        .ToList();

                    if (newQueries.Any()) {
                        _logger.LogInformation("AI generated {Count} new suggestion queries: {Queries}",
                            newQueries.Count, string.Join(", ", newQueries));
                        await _notificationService.SendStatusUpdateAsync(userEmail, "processing", $"AI generated {newQueries.Count} new suggestion strategies based on context");
                        return newQueries;
                    }
                }
            }
        } catch (Exception ex) {
            _logger.LogWarning(ex, "Failed to get AI adaptation for suggestions, continuing with existing queries");
        }

        return currentSearchQueries;
    }
}