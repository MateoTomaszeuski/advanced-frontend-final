using System.Text.Json;
using API.DTOs.Spotify;
using API.Interfaces;
using API.Models.AI;
using API.Services.Helpers;

namespace API.Services.Helpers;

public class TrackDiscoveryHelper : ITrackDiscoveryHelper {
    private readonly ISpotifyService _spotifyService;
    private readonly IAIService _aiService;
    private readonly IAgentNotificationService _notificationService;
    private readonly IDiscoveryQueryGenerator _queryGenerator;
    private readonly ILogger<TrackDiscoveryHelper> _logger;

    public TrackDiscoveryHelper(
        ISpotifyService spotifyService,
        IAIService aiService,
        IAgentNotificationService notificationService,
        IDiscoveryQueryGenerator queryGenerator,
        ILogger<TrackDiscoveryHelper> logger) {
        _spotifyService = spotifyService;
        _aiService = aiService;
        _notificationService = notificationService;
        _queryGenerator = queryGenerator;
        _logger = logger;
    }

    public async Task<List<SpotifyTrack>> DiscoverWithoutSavedTracksAsync(
        string accessToken,
        int limit,
        HashSet<string> savedTrackIds) {
        _logger.LogWarning("User has no saved tracks, using AI to discover music based on popular genres");

        var aiMessages = new List<AIMessage>
        {
            new AIMessage("system", @"You are a music expert. Generate a diverse set of music genres for discovering new music. Return ONLY a JSON array of genre names.

Return your response in this format:
{
  ""genres"": [""pop"", ""rock"", ""hip-hop"", ""indie"", ""electronic""]
}

IMPORTANT: Provide a unique, diverse mix of genres each time - avoid repeating the same genres."),
            new AIMessage("user", $"[Request #{DateTime.UtcNow.Ticks}] Generate 5 diverse music genres for music discovery")
        };

        var aiResponse = await _aiService.GetChatCompletionAsync(aiMessages);
        var genres = new[] { "pop", "rock", "indie", "electronic", "jazz" };

        if (!string.IsNullOrEmpty(aiResponse.Response)) {
            try {
                var jsonStart = aiResponse.Response.IndexOf('{');
                var jsonEnd = aiResponse.Response.LastIndexOf('}');
                if (jsonStart >= 0 && jsonEnd > jsonStart) {
                    var jsonStr = aiResponse.Response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    var aiData = JsonSerializer.Deserialize<JsonElement>(jsonStr);
                    var genreArray = aiData.GetProperty("genres");
                    genres = genreArray.EnumerateArray().Select(g => g.GetString() ?? "pop").ToArray();
                }
            } catch (Exception ex) {
                _logger.LogWarning(ex, "Failed to parse AI genres, using defaults");
            }
        }

        var allTracks = new List<SpotifyTrack>();
        var trackIds = new HashSet<string>();
        var trackUris = new HashSet<string>();
        var trackKeys = new HashSet<string>();

        foreach (var genre in genres) {
            if (allTracks.Count >= limit) break;

            var searchResults = await _spotifyService.SearchTracksAsync(
                accessToken,
                $"genre:{genre}",
                Math.Min(50, (limit - allTracks.Count) * 2)
            );

            foreach (var track in searchResults) {
                var trackKey = TrackDeduplicationHelper.GetTrackKey(track);
                bool hasSimilarTitle = TrackDeduplicationHelper.IsDuplicateTrack(track, allTracks);

                if (!savedTrackIds.Contains(track.Id)
                    && !hasSimilarTitle
                    && trackIds.Add(track.Id)
                    && trackUris.Add(track.Uri)
                    && trackKeys.Add(trackKey)) {
                    allTracks.Add(track);
                    if (allTracks.Count >= limit * 2) break;
                }
            }
        }

        return allTracks;
    }

    public async Task<List<SpotifyTrack>> DiscoverFromSearchQueriesAsync(
        string accessToken,
        List<string> searchQueries,
        int limit,
        HashSet<string> savedTrackIds,
        string userEmail) {
        var allTracks = new List<SpotifyTrack>();
        var trackIds = new HashSet<string>();
        var trackUris = new HashSet<string>();
        var trackKeys = new HashSet<string>();

        var searchIterations = 0;
        const int maxSearchIterations = 10;
        var currentQueries = searchQueries.ToList();

        while (allTracks.Count < limit && searchIterations < maxSearchIterations) {
            searchIterations++;

            foreach (var query in currentQueries.ToList()) {
                if (allTracks.Count >= limit) break;

                _logger.LogInformation("Search iteration {Iteration}, query: '{Query}'", searchIterations, query);
                await _notificationService.SendStatusUpdateAsync(userEmail, "processing",
                    $"Discovery iteration {searchIterations}: Searching with query '{query}'");

                var searchResults = await _spotifyService.SearchTracksAsync(accessToken, query, 50);

                var tracksFoundInQuery = 0;
                foreach (var track in searchResults) {
                    var trackKey = TrackDeduplicationHelper.GetTrackKey(track);
                    bool hasSimilarTitle = TrackDeduplicationHelper.IsDuplicateTrack(track, allTracks);

                    if (!savedTrackIds.Contains(track.Id)
                        && !hasSimilarTitle
                        && trackIds.Add(track.Id)
                        && trackUris.Add(track.Uri)
                        && trackKeys.Add(trackKey)) {
                        allTracks.Add(track);
                        tracksFoundInQuery++;
                        if (allTracks.Count >= limit) break;
                    }
                }

                _logger.LogInformation("After query '{Query}': found {NewTracks} new tracks, total {Count} unique tracks",
                    query, tracksFoundInQuery, allTracks.Count);
                await _notificationService.SendStatusUpdateAsync(userEmail, "processing",
                    $"After query '{query}': found {tracksFoundInQuery} new tracks, total {allTracks.Count} unique tracks");
            }

            if (allTracks.Count < limit && searchIterations < maxSearchIterations) {
                _logger.LogInformation("Need {Missing} more tracks, generating new queries...", limit - allTracks.Count);
                currentQueries = await _queryGenerator.AdaptQueriesAsync(
                    await GetTopTracksFromUser(allTracks),
                    currentQueries,
                    allTracks.Count,
                    limit,
                    userEmail
                );
            }
        }

        return allTracks;
    }

    private Task<SpotifyTrack[]> GetTopTracksFromUser(List<SpotifyTrack> tracks) {
        return Task.FromResult(tracks.Take(5).ToArray());
    }

    public async Task<List<SpotifyTrack>> FallbackToRecommendationsAsync(
        string accessToken,
        SpotifyTrack[] seedTracks,
        int limit,
        List<SpotifyTrack> existingTracks,
        HashSet<string> savedTrackIds) {
        _logger.LogInformation("Using Spotify recommendations as fallback, need {Missing} more tracks",
            limit - existingTracks.Count);

        var trackIds = existingTracks.Select(t => t.Id).ToHashSet();

        try {
            var seeds = seedTracks.Take(5).Select(t => t.Id).ToArray();
            var recommendationsLimit = Math.Min(100, Math.Max(20, (limit - existingTracks.Count) * 2));

            var recommendations = await _spotifyService.GetRecommendationsAsync(
                accessToken,
                seeds,
                recommendationsLimit
            );

            foreach (var track in recommendations) {
                if (!savedTrackIds.Contains(track.Id) && trackIds.Add(track.Id)) {
                    existingTracks.Add(track);
                    if (existingTracks.Count >= limit * 2) break;
                }
            }

            _logger.LogInformation("After recommendations: {Count} unique new tracks", existingTracks.Count);
        } catch (Exception ex) {
            _logger.LogWarning(ex, "Failed to get recommendations from Spotify, continuing with search results only");
        }

        return existingTracks;
    }
}