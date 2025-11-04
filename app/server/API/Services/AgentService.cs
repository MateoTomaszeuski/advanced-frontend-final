using System.Text.Json;
using API.DTOs.Agent;
using API.DTOs.Spotify;
using API.Models;
using API.Repositories;
using API.Services.Helpers;

namespace API.Services;

public interface IAgentService
{
    Task<AgentActionResponse> CreateSmartPlaylistAsync(User user, CreateSmartPlaylistRequest request, int conversationId);
    Task<AgentActionResponse> DiscoverNewMusicAsync(User user, DiscoverNewMusicRequest request, int conversationId);
}

public class AgentService : IAgentService
{
    private readonly IAgentActionRepository _actionRepository;
    private readonly ISpotifyService _spotifyService;
    private readonly IAIService _aiService;
    private readonly ILogger<AgentService> _logger;
    private readonly TrackFilterHelper _trackFilterHelper;

    public AgentService(
        IAgentActionRepository actionRepository,
        ISpotifyService spotifyService,
        IAIService aiService,
        ILogger<AgentService> logger)
    {
        _actionRepository = actionRepository;
        _spotifyService = spotifyService;
        _aiService = aiService;
        _logger = logger;
        _trackFilterHelper = new TrackFilterHelper(spotifyService);
    }

    public async Task<AgentActionResponse> CreateSmartPlaylistAsync(
        User user,
        CreateSmartPlaylistRequest request,
        int conversationId)
    {
        var action = new AgentAction
        {
            ConversationId = conversationId,
            ActionType = "CreateSmartPlaylist",
            Status = "Processing",
            InputPrompt = request.Prompt,
            Parameters = JsonSerializer.SerializeToDocument(request),
            CreatedAt = DateTime.UtcNow
        };

        action = await _actionRepository.CreateAsync(action);

        try
        {
            if (string.IsNullOrEmpty(user.SpotifyAccessToken))
            {
                throw new InvalidOperationException("User has not connected their Spotify account");
            }

            _logger.LogInformation("Creating smart playlist for user {Email} with prompt: {Prompt}",
                user.Email, request.Prompt);

            var aiMessages = new List<Models.AI.AIMessage>
            {
                new Models.AI.AIMessage("system", @"You are a music expert assistant. Generate a creative and catchy playlist name, and an effective Spotify search query based on the user's description.

IMPORTANT: Spotify search has LIMITED syntax support. Only use these proven search patterns:
- Genre keywords: 'funk', 'rap', 'trap', 'jazz', 'rock', 'electronic', 'pop'
- Genre filter: 'genre:rock', 'genre:jazz', 'genre:electronic'
- Artist names: 'artist:Radiohead', or just 'Radiohead'
- Track names: 'track:Breathe'
- Year: 'year:2020'

DO NOT use these (they don't work reliably):
- mood:, energy:, tempo:, instrumentalness:, valence:
- Complex boolean operators
- Descriptive adjectives as literal search terms (upbeat, chill, energetic, etc.)

CRITICAL: Translate descriptive words to actual music characteristics:
- 'upbeat' → use genres like 'funk', 'pop', 'dance' (NOT the word 'upbeat')
- 'chill' → use 'indie', 'ambient', 'acoustic' (NOT the word 'chill')
- 'energetic' → use 'rock', 'electronic', 'punk' (NOT the word 'energetic')
- 'relaxing' → use 'classical', 'ambient', 'jazz' (NOT the word 'relaxing')

For best results:
1. Extract actual GENRES from the user's request
2. If user says descriptive words (upbeat, chill, etc.), convert to relevant genres
3. Include artist names if mentioned
4. Keep query focused on genres and concrete terms

Examples of GOOD queries:
- User: 'upbeat funk and pop' → Query: 'funk pop dance'
- User: 'chill indie music' → Query: 'indie acoustic ambient'
- User: 'energetic workout music' → Query: 'rock electronic hip-hop'
- User: 'relaxing piano' → Query: 'classical piano jazz'
- User: 'funk, rap and argentinian trap' → Query: 'funk rap trap latino'

Return your response in the following JSON format only:
{
  ""playlistName"": ""Creative Playlist Name"",
  ""searchQuery"": ""genres and concrete terms only"",
  ""description"": ""Brief description of the playlist""
}"),
                new Models.AI.AIMessage("user", $"Create a playlist for: {request.Prompt}")
            };

            var aiResponse = await _aiService.GetChatCompletionAsync(aiMessages);
            
            string playlistName;
            string searchQuery;
            string playlistDescription;

            if (string.IsNullOrEmpty(aiResponse.Response) || aiResponse.Error != null)
            {
                _logger.LogWarning("AI failed to generate playlist metadata, using fallback. Error: {Error}", aiResponse.Error);
                playlistName = PlaylistHelper.GeneratePlaylistName(request.Prompt);
                searchQuery = PlaylistHelper.ParsePromptToSearchQuery(request.Prompt);
                playlistDescription = $"AI-generated playlist: {request.Prompt}";
            }
            else
            {
                try
                {
                    var jsonStart = aiResponse.Response.IndexOf('{');
                    var jsonEnd = aiResponse.Response.LastIndexOf('}');
                    if (jsonStart >= 0 && jsonEnd > jsonStart)
                    {
                        var jsonStr = aiResponse.Response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                        var aiData = JsonSerializer.Deserialize<JsonElement>(jsonStr);
                        playlistName = aiData.GetProperty("playlistName").GetString() ?? PlaylistHelper.GeneratePlaylistName(request.Prompt);
                        searchQuery = aiData.GetProperty("searchQuery").GetString() ?? request.Prompt;
                        playlistDescription = aiData.GetProperty("description").GetString() ?? $"AI-generated playlist: {request.Prompt}";
                    }
                    else
                    {
                        throw new Exception("No JSON found in AI response");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse AI response, using fallback. Response: {Response}", aiResponse.Response);
                    playlistName = PlaylistHelper.GeneratePlaylistName(request.Prompt);
                    searchQuery = PlaylistHelper.ParsePromptToSearchQuery(request.Prompt);
                    playlistDescription = $"AI-generated playlist: {request.Prompt}";
                }
            }

            _logger.LogInformation("Using playlist name: {PlaylistName}, search query: {SearchQuery}", playlistName, searchQuery);

            var requestedTrackCount = request.Preferences?.MaxTracks ?? 20;
            var allTracks = new List<SpotifyTrack>();
            var trackIds = new HashSet<string>();

            var initialTracks = await _spotifyService.SearchTracksAsync(
                user.SpotifyAccessToken,
                searchQuery,
                Math.Min(50, requestedTrackCount * 2)
            );

            foreach (var track in initialTracks)
            {
                if (trackIds.Add(track.Id))
                {
                    allTracks.Add(track);
                }
            }

            _logger.LogInformation("Initial search returned {Count} unique tracks", allTracks.Count);

            if (allTracks.Count < requestedTrackCount)
            {
                _logger.LogInformation("Need {Missing} more tracks, searching with broader queries", requestedTrackCount - allTracks.Count);

                var searchWords = searchQuery.Split(new[] { ' ', ',', '-' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => !string.IsNullOrWhiteSpace(w) && w.Length > 2)
                    .ToArray();

                var fallbackQueries = new List<string>();
                
                if (searchWords.Length > 2)
                {
                    fallbackQueries.Add(string.Join(" ", searchWords.Take(2)));
                }
                
                var genreWords = searchWords.Where(w => !w.Contains("genre:") && 
                    (w.Contains("funk") || w.Contains("rap") || w.Contains("trap") || w.Contains("rock") || 
                     w.Contains("pop") || w.Contains("jazz") || w.Contains("indie") || w.Contains("electronic") ||
                     w.Contains("hip") || w.Contains("latino") || w.Contains("reggaeton") || w.Contains("dance")))
                    .ToArray();
                
                if (genreWords.Any())
                {
                    fallbackQueries.Add(string.Join(" ", genreWords));
                }
                
                fallbackQueries.Add(searchWords.FirstOrDefault() ?? request.Prompt);

                foreach (var fallbackQuery in fallbackQueries.Distinct())
                {
                    if (allTracks.Count >= requestedTrackCount) break;

                    _logger.LogInformation("Trying fallback query: '{Query}'", fallbackQuery);

                    var additionalTracks = await _spotifyService.SearchTracksAsync(
                        user.SpotifyAccessToken,
                        fallbackQuery,
                        Math.Min(50, (requestedTrackCount - allTracks.Count) * 2)
                    );

                    foreach (var track in additionalTracks)
                    {
                        if (trackIds.Add(track.Id) && allTracks.Count < requestedTrackCount * 2)
                        {
                            allTracks.Add(track);
                        }
                    }

                    _logger.LogInformation("After fallback query '{Query}': {Count} unique tracks", fallbackQuery, allTracks.Count);
                }
            }

            var tracks = allTracks.ToArray();

            if (request.Preferences != null)
            {
                tracks = await _trackFilterHelper.FilterByPreferencesAsync(
                    user.SpotifyAccessToken, 
                    tracks, 
                    request.Preferences
                );
                
                _logger.LogInformation("After filtering by preferences: {Count} tracks", tracks.Length);
            }

            tracks = tracks.Take(requestedTrackCount).ToArray();

            _logger.LogInformation("Final track count: {Count} (requested: {Requested})", tracks.Length, requestedTrackCount);

            var userId = await _spotifyService.GetCurrentUserIdAsync(user.SpotifyAccessToken);

            var playlist = await _spotifyService.CreatePlaylistAsync(
                user.SpotifyAccessToken,
                userId,
                new CreatePlaylistRequest(playlistName, playlistDescription, false)
            );

            if (tracks.Length > 0)
            {
                var trackUris = tracks.Select(t => t.Uri).ToArray();
                await _spotifyService.AddTracksToPlaylistAsync(
                    user.SpotifyAccessToken,
                    playlist.Id,
                    trackUris
                );
            }

            var result = new
            {
                playlistId = playlist.Id,
                playlistName = playlist.Name,
                playlistUri = playlist.Uri,
                trackCount = tracks.Length,
                tracks = tracks.Select(t => new
                {
                    t.Id,
                    t.Name,
                    Artists = string.Join(", ", t.Artists.Select(a => a.Name)),
                    t.Uri
                }).ToArray()
            };

            action.Status = "Completed";
            action.Result = JsonSerializer.SerializeToDocument(result);
            action.CompletedAt = DateTime.UtcNow;
            await _actionRepository.UpdateAsync(action);

            _logger.LogInformation("Successfully created playlist {PlaylistId} with {TrackCount} tracks",
                playlist.Id, tracks.Length);

            return new AgentActionResponse(action.Id, action.ActionType, action.Status, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating smart playlist for user {Email}", user.Email);

            action.Status = "Failed";
            action.ErrorMessage = ex.Message;
            action.CompletedAt = DateTime.UtcNow;
            await _actionRepository.UpdateAsync(action);

            return new AgentActionResponse(action.Id, action.ActionType, action.Status, null, ex.Message);
        }
    }

    public async Task<AgentActionResponse> DiscoverNewMusicAsync(
        User user,
        DiscoverNewMusicRequest request,
        int conversationId)
    {
        var action = new AgentAction
        {
            ConversationId = conversationId,
            ActionType = "DiscoverNewMusic",
            Status = "Processing",
            Parameters = JsonSerializer.SerializeToDocument(request),
            CreatedAt = DateTime.UtcNow
        };

        action = await _actionRepository.CreateAsync(action);

        try
        {
            if (string.IsNullOrEmpty(user.SpotifyAccessToken))
            {
                throw new InvalidOperationException("User has not connected their Spotify account");
            }

            _logger.LogInformation("Discovering new music for user {Email}, requested: {Limit} tracks", user.Email, request.Limit);

            var savedTracks = await _spotifyService.GetUserSavedTracksAsync(user.SpotifyAccessToken, 50);
            var savedTrackIds = savedTracks.Select(t => t.Id).ToHashSet();

            _logger.LogInformation("User has {Count} saved tracks", savedTrackIds.Count);

            var allDiscoveredTracks = new List<SpotifyTrack>();
            var discoveredTrackIds = new HashSet<string>();

            var topSavedTracks = savedTracks
                .OrderByDescending(t => t.Popularity)
                .Take(5)
                .ToArray();

            if (topSavedTracks.Length == 0)
            {
                _logger.LogWarning("User has no saved tracks, using AI to discover music based on popular genres");
                
                var aiMessages = new List<Models.AI.AIMessage>
                {
                    new Models.AI.AIMessage("system", @"You are a music expert. Generate a diverse set of music genres for discovering new music. Return ONLY a JSON array of genre names.

Return your response in this format:
{
  ""genres"": [""pop"", ""rock"", ""hip-hop"", ""indie"", ""electronic""]
}"),
                    new Models.AI.AIMessage("user", "Generate 5 diverse music genres for music discovery")
                };

                var aiResponse = await _aiService.GetChatCompletionAsync(aiMessages);
                
                var genres = new[] { "pop", "rock", "indie", "electronic", "jazz" };
                
                if (!string.IsNullOrEmpty(aiResponse.Response))
                {
                    try
                    {
                        var jsonStart = aiResponse.Response.IndexOf('{');
                        var jsonEnd = aiResponse.Response.LastIndexOf('}');
                        if (jsonStart >= 0 && jsonEnd > jsonStart)
                        {
                            var jsonStr = aiResponse.Response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                            var aiData = JsonSerializer.Deserialize<JsonElement>(jsonStr);
                            var genreArray = aiData.GetProperty("genres");
                            genres = genreArray.EnumerateArray().Select(g => g.GetString() ?? "pop").ToArray();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse AI genres, using defaults");
                    }
                }

                foreach (var genre in genres)
                {
                    if (allDiscoveredTracks.Count >= request.Limit) break;

                    var searchResults = await _spotifyService.SearchTracksAsync(
                        user.SpotifyAccessToken,
                        $"genre:{genre}",
                        Math.Min(50, (request.Limit - allDiscoveredTracks.Count) * 2)
                    );

                    foreach (var track in searchResults)
                    {
                        if (!savedTrackIds.Contains(track.Id) && discoveredTrackIds.Add(track.Id))
                        {
                            allDiscoveredTracks.Add(track);
                            if (allDiscoveredTracks.Count >= request.Limit * 2) break;
                        }
                    }
                }
            }
            else
            {
                var aiMessages = new List<Models.AI.AIMessage>
                {
                    new Models.AI.AIMessage("system", @"You are a music expert assistant. Analyze the user's favorite tracks and generate search queries to discover similar but new music.

Generate 3-5 search queries based on the genres and artists. Use actual genres and artist names.

Return your response in the following JSON format:
{
  ""queries"": [""indie rock alternative"", ""electronic dance pop"", ""hip-hop rap""]
}"),
                    new Models.AI.AIMessage("user", $"User's top tracks: {string.Join(", ", topSavedTracks.Select(t => $"{t.Name} by {string.Join(", ", t.Artists.Select(a => a.Name))}"))}. Generate search queries to discover similar music.")
                };

                var aiResponse = await _aiService.GetChatCompletionAsync(aiMessages);
                
                var searchQueries = new List<string>();

                if (!string.IsNullOrEmpty(aiResponse.Response))
                {
                    try
                    {
                        var jsonStart = aiResponse.Response.IndexOf('{');
                        var jsonEnd = aiResponse.Response.LastIndexOf('}');
                        if (jsonStart >= 0 && jsonEnd > jsonStart)
                        {
                            var jsonStr = aiResponse.Response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                            var aiData = JsonSerializer.Deserialize<JsonElement>(jsonStr);
                            var queryArray = aiData.GetProperty("queries");
                            searchQueries = queryArray.EnumerateArray().Select(q => q.GetString() ?? "").Where(q => !string.IsNullOrEmpty(q)).ToList();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse AI queries, using fallback");
                    }
                }

                if (searchQueries.Count == 0)
                {
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

                _logger.LogInformation("Using {Count} search queries for discovery", searchQueries.Count);

                foreach (var query in searchQueries)
                {
                    if (allDiscoveredTracks.Count >= request.Limit) break;

                    _logger.LogInformation("Searching with query: '{Query}'", query);

                    var searchResults = await _spotifyService.SearchTracksAsync(
                        user.SpotifyAccessToken,
                        query,
                        Math.Min(50, (request.Limit - allDiscoveredTracks.Count) * 2)
                    );

                    foreach (var track in searchResults)
                    {
                        if (!savedTrackIds.Contains(track.Id) && discoveredTrackIds.Add(track.Id))
                        {
                            allDiscoveredTracks.Add(track);
                            if (allDiscoveredTracks.Count >= request.Limit * 2) break;
                        }
                    }

                    _logger.LogInformation("After query '{Query}': {Count} unique new tracks", query, allDiscoveredTracks.Count);
                }

                if (allDiscoveredTracks.Count < request.Limit && topSavedTracks.Length > 0)
                {
                    _logger.LogInformation("Using Spotify recommendations as fallback");

                    var seedTracks = topSavedTracks.Take(5).Select(t => t.Id).ToArray();
                    
                    var batchesNeeded = (int)Math.Ceiling((request.Limit - allDiscoveredTracks.Count) / 100.0);
                    
                    for (int i = 0; i < batchesNeeded && allDiscoveredTracks.Count < request.Limit * 2; i++)
                    {
                        var recommendations = await _spotifyService.GetRecommendationsAsync(
                            user.SpotifyAccessToken,
                            seedTracks,
                            Math.Min(100, (request.Limit - allDiscoveredTracks.Count) * 2)
                        );

                        foreach (var track in recommendations)
                        {
                            if (!savedTrackIds.Contains(track.Id) && discoveredTrackIds.Add(track.Id))
                            {
                                allDiscoveredTracks.Add(track);
                                if (allDiscoveredTracks.Count >= request.Limit * 2) break;
                            }
                        }
                    }
                }
            }

            var newTracks = allDiscoveredTracks.Take(request.Limit).ToArray();

            _logger.LogInformation("Final discovery: {Count} unique new tracks (requested: {Requested})", newTracks.Length, request.Limit);

            var userId = await _spotifyService.GetCurrentUserIdAsync(user.SpotifyAccessToken);
            var playlistName = $"Discover Weekly - {DateTime.UtcNow:MMM dd, yyyy}";
            var playlistDescription = "AI-generated music discovery based on your listening habits";

            var playlist = await _spotifyService.CreatePlaylistAsync(
                user.SpotifyAccessToken,
                userId,
                new CreatePlaylistRequest(playlistName, playlistDescription, false)
            );

            if (newTracks.Length > 0)
            {
                var trackUris = newTracks.Select(t => t.Uri).ToArray();
                await _spotifyService.AddTracksToPlaylistAsync(
                    user.SpotifyAccessToken,
                    playlist.Id,
                    trackUris
                );
            }

            var result = new
            {
                playlistId = playlist.Id,
                playlistName = playlist.Name,
                playlistUri = playlist.Uri,
                trackCount = newTracks.Length,
                tracks = newTracks.Select(t => new
                {
                    t.Id,
                    t.Name,
                    Artists = string.Join(", ", t.Artists.Select(a => a.Name)),
                    t.Uri,
                    t.Popularity
                }).ToArray()
            };

            action.Status = "Completed";
            action.Result = JsonSerializer.SerializeToDocument(result);
            action.CompletedAt = DateTime.UtcNow;
            await _actionRepository.UpdateAsync(action);

            _logger.LogInformation("Successfully discovered {TrackCount} new tracks for user {Email}",
                newTracks.Length, user.Email);

            return new AgentActionResponse(action.Id, action.ActionType, action.Status, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering new music for user {Email}", user.Email);

            action.Status = "Failed";
            action.ErrorMessage = ex.Message;
            action.CompletedAt = DateTime.UtcNow;
            await _actionRepository.UpdateAsync(action);

            return new AgentActionResponse(action.Id, action.ActionType, action.Status, null, ex.Message);
        }
    }
}
