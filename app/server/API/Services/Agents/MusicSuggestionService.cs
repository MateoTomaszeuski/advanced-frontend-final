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

        await _notificationService.SendStatusUpdateAsync(user.Email, "processing", "Analyzing playlist and checking your music library...");

        var playlist = await _playlistService.GetPlaylistAsync(accessToken, request.PlaylistId);
        var playlistTracks = await _trackService.GetPlaylistTracksAsync(accessToken, request.PlaylistId);
        var existingTrackIds = playlistTracks.Select(t => t.Id).ToHashSet();

        var userPlaylists = await _spotifyService.GetUserPlaylistsAsync(accessToken, 50);
        _logger.LogInformation("Checking {Count} user playlists to avoid duplicates", userPlaylists.Length);

        foreach (var userPlaylist in userPlaylists) {
            try {
                var tracks = await _trackService.GetPlaylistTracksAsync(accessToken, userPlaylist.Id);
                foreach (var track in tracks) {
                    existingTrackIds.Add(track.Id);
                }
            } catch (Exception ex) {
                _logger.LogWarning(ex, "Failed to get tracks from playlist {PlaylistId}", userPlaylist.Id);
            }
        }

        _logger.LogInformation("Total tracks to exclude: {Count}", existingTrackIds.Count);
        await _notificationService.SendStatusUpdateAsync(user.Email, "processing", 
            $"Found {existingTrackIds.Count} existing tracks across all playlists to exclude");

        await _notificationService.SendStatusUpdateAsync(user.Email, "processing", 
            "Asking AI to analyze playlist and generate search strategies...");

        var topTracks = playlistTracks.OrderByDescending(t => t.Popularity).Take(10).ToArray();
        var allSuggestions = new List<SuggestedTrack>();
        var suggestionIds = new HashSet<string>();
        var suggestionAttempts = 0;
        const int maxSuggestionAttempts = 5;
        var targetCount = Math.Min(request.Limit, 250);

        while (allSuggestions.Count < targetCount && suggestionAttempts < maxSuggestionAttempts) {
            suggestionAttempts++;
            var remainingNeeded = targetCount - allSuggestions.Count;
            
            _logger.LogInformation("Suggestion attempt {Attempt}: Need {Remaining} more tracks (have {Current})", 
                suggestionAttempts, remainingNeeded, allSuggestions.Count);
            
            await _notificationService.SendStatusUpdateAsync(user.Email, "processing", 
                $"Suggestion attempt {suggestionAttempts}: Looking for {remainingNeeded} more unique tracks...");

            var (searchQueries, explanation) = await GenerateSuggestionQueriesAsync(playlist, topTracks, request.Context);
            await _notificationService.SendStatusUpdateAsync(user.Email, "processing", 
                $"AI generated {searchQueries.Count} search strategies based on context: '{request.Context}'");
            var currentSearchQueries = searchQueries.ToList();
            var searchIterations = 0;
            const int maxSearchIterations = 3;

            var attemptSuggestions = new List<SuggestedTrack>();
            
            while (attemptSuggestions.Count < remainingNeeded && searchIterations < maxSearchIterations) {
                searchIterations++;

                foreach (var query in currentSearchQueries.ToList()) {
                    if (attemptSuggestions.Count >= remainingNeeded) break;

                    _logger.LogInformation("Searching suggestions (attempt {Attempt}, iteration {Iteration}) with query: '{Query}'", 
                        suggestionAttempts, searchIterations, query);
                    await _notificationService.SendStatusUpdateAsync(user.Email, "processing", 
                        $"Searching with query '{query}'");

                    var searchResults = await _spotifyService.SearchTracksAsync(accessToken, query, 50);

                    var tracksFoundInThisQuery = 0;
                    foreach (var track in searchResults) {
                        if (!existingTrackIds.Contains(track.Id) && suggestionIds.Add(track.Id)) {
                            var suggestedTrack = new SuggestedTrack(
                                track.Id,
                                track.Name,
                                track.Artists.Select(a => a.Name).ToArray(),
                                track.Uri,
                                $"Matches '{query}' - {explanation}",
                                track.Popularity
                            );
                            attemptSuggestions.Add(suggestedTrack);
                            existingTrackIds.Add(track.Id);
                            tracksFoundInThisQuery++;

                            if (attemptSuggestions.Count >= remainingNeeded) break;
                        }
                    }

                    _logger.LogInformation("After query '{Query}': found {NewTracks} new suggestions, attempt total {Count}",
                        query, tracksFoundInThisQuery, attemptSuggestions.Count);
                    await _notificationService.SendStatusUpdateAsync(user.Email, "processing", 
                        $"After query '{query}': found {tracksFoundInThisQuery} new suggestions, total {allSuggestions.Count + attemptSuggestions.Count}/{targetCount}");
                }

                if (attemptSuggestions.Count < remainingNeeded && searchIterations < maxSearchIterations) {
                    await _notificationService.SendStatusUpdateAsync(user.Email, "processing", 
                        $"Iteration {searchIterations}/{maxSearchIterations}: Found {attemptSuggestions.Count}/{remainingNeeded} needed. Asking AI for new strategies...");
                    currentSearchQueries = await AdaptSuggestionQueriesAsync(playlist, topTracks, request.Context, currentSearchQueries, attemptSuggestions.Count, remainingNeeded, user.Email);
                }
            }

            allSuggestions.AddRange(attemptSuggestions);
            _logger.LogInformation("Suggestion attempt {Attempt}: Found {Unique} unique tracks, total: {Total}", 
                suggestionAttempts, attemptSuggestions.Count, allSuggestions.Count);
            await _notificationService.SendStatusUpdateAsync(user.Email, "processing", 
                $"Attempt {suggestionAttempts} complete: Found {attemptSuggestions.Count} new suggestions. Total: {allSuggestions.Count}/{targetCount}");

            if (attemptSuggestions.Count == 0) {
                _logger.LogWarning("No new unique tracks found in attempt {Attempt}, stopping", suggestionAttempts);
                break;
            }
        }

        _logger.LogInformation("Generated {Count} suggestions for playlist {PlaylistId}",
            allSuggestions.Count, request.PlaylistId);

        await _notificationService.SendStatusUpdateAsync(user.Email, "completed", 
            $"Successfully generated {allSuggestions.Count} suggestions for '{playlist.Name}'!", 
            new { suggestionCount = allSuggestions.Count, context = request.Context });

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
- artist: 'artist:""Artist Name""'
- year: 'year:2020' or 'year:1980-1990'
- genre: 'genre:rock' 'genre:jazz' 'genre:electronic' 'genre:hip-hop' 'genre:pop' 'genre:indie' 'genre:metal' 'genre:country' 'genre:classical' 'genre:reggae' 'genre:blues' 'genre:soul' 'genre:funk' 'genre:punk' 'genre:folk' 'genre:r-n-b' 'genre:dance' 'genre:latin' 'genre:afrobeat'

REASONING PROCESS:
1. Analyze what genres/artists are in the playlist
2. Think about what the user's context means (workout → high energy, party → upbeat/danceable, study → calm/focus, etc.)
3. Identify famous artists that match BOTH the playlist style AND the context
4. Build SIMPLE queries - famous artists or genre+keywords

QUERY BUILDING STRATEGY:
- Keep queries simple and focused
- Use well-known artists that match the playlist + context
- Or use genre + simple mood keywords
- Avoid long strings of similar adjectives
- One clear direction per query

Examples:
- Playlist: Indie rock, Context: 'workout' → 'artist:""The Strokes""', 'energetic genre:rock', 'artist:""Arctic Monkeys""'
- Playlist: Electronic, Context: 'party' → 'artist:""Daft Punk""', 'genre:dance year:2015-2024', 'artist:""Calvin Harris""'
- Playlist: Jazz, Context: 'study' → 'calm genre:jazz', 'artist:""Bill Evans""', 'peaceful piano genre:classical'

Generate 5-7 simple search queries that match the context while fitting the playlist's style.

Return your response in the following JSON format:
{
  ""queries"": [""query1"", ""query2"", ""query3""],
  ""explanation"": ""Brief explanation of the suggestion strategy""
}

IMPORTANT: Keep queries simple - famous artists and clear genre+mood combos work best!"),
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

REASONING PROCESS:
1. Look at what queries were already tried and the playlist style
2. Think of OTHER well-known artists that match the context
3. Consider slightly different time periods or sub-genres
4. Keep queries SIMPLE - one artist or one genre+keyword combo per query

The current queries aren't finding enough NEW tracks. Generate 5-7 DIFFERENT but SIMPLE search queries:
- Try different well-known artists that match the context + playlist style
- Use related genres with mood keywords
- Expand time periods slightly if relevant
- Keep each query simple and focused

Examples:
- Already tried: 'artist:""Daft Punk""'
  → New: 'artist:""Justice""', 'artist:""Kavinsky""', 'genre:electronic year:2010-2020'
- Already tried: 'energetic genre:rock'
  → New: 'artist:""Queens of the Stone Age""', 'artist:""Royal Blood""', 'genre:rock year:2015-2024'

Return your response in this JSON format:
{
  ""queries"": [""query1"", ""query2"", ""query3""]
}

IMPORTANT: Keep it simple - famous artists work best!"),
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