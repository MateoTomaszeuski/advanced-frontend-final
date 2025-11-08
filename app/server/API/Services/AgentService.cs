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
    Task<RemoveDuplicatesResponse> ScanForDuplicatesAsync(User user, string playlistId, int conversationId);
    Task<AgentActionResponse> ConfirmRemoveDuplicatesAsync(User user, ConfirmRemoveDuplicatesRequest request, int conversationId);
    Task<SuggestMusicResponse> SuggestMusicByContextAsync(User user, SuggestMusicRequest request, int conversationId);
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

SPOTIFY SEARCH API - OFFICIAL SUPPORTED FILTERS:
When searching for tracks, you can use these field filters:
- album: 'album:Dookie' or 'album:""Dookie""'
- artist: 'artist:Radiohead' or 'artist:""Miles Davis""'
- track: 'track:Breathe' or 'track:""Smells Like Teen Spirit""'
- year: 'year:2020' or year range 'year:1980-1990'
- genre: 'genre:rock' 'genre:jazz' 'genre:electronic' 'genre:hip-hop' 'genre:pop' 'genre:indie' 'genre:metal' 'genre:country' 'genre:classical' 'genre:reggae' 'genre:blues' 'genre:soul' 'genre:funk' 'genre:punk' 'genre:folk' 'genre:r-n-b' 'genre:dance' 'genre:latin' 'genre:afrobeat'
- isrc: 'isrc:USUM71703861' (International Standard Recording Code - rarely needed)

IMPORTANT RULES:
1. You can combine filters: 'genre:rock year:2010-2020'
2. You can use keywords + filters: 'upbeat genre:funk artist:Prince'
3. Use quotes for multi-word values: 'artist:""Daft Punk""'
4. Use ONLY ONE genre filter per query - multiple genre filters don't work well
5. DO NOT use made-up filters like: mood:, energy:, tempo:, valence:, danceability:, instrumentalness:

QUERY BUILDING STRATEGY:
1. If user mentions specific artists → use 'artist:ArtistName'
2. If user mentions ONE primary genre → use 'genre:genrename'
3. If user mentions time period → use 'year:YYYY' or 'year:YYYY-YYYY'
4. Add descriptive keywords (upbeat, chill, energetic) as regular text alongside filters
5. Mix keywords with ONE genre filter for best results

Examples of CORRECT queries:
- User: 'upbeat funk and pop' → Query: 'upbeat dance funk pop genre:funk'
- User: 'chill indie music' → Query: 'chill mellow acoustic genre:indie'
- User: 'energetic workout music' → Query: 'energetic workout genre:rock'
- User: 'relaxing piano from 2010s' → Query: 'relaxing piano genre:classical year:2010-2019'
- User: 'funk, rap and argentinian trap' → Query: 'funk rap trap latino genre:hip-hop'
- User: '80s rock hits' → Query: 'hits classic genre:rock year:1980-1989'
- User: 'songs like Radiohead' → Query: 'alternative rock artist:Radiohead'

Return your response in the following JSON format only:
{
  ""playlistName"": ""Creative Playlist Name"",
  ""searchQuery"": ""keywords and filters combined"",
  ""description"": ""Brief description of the playlist""
}

IMPORTANT: Each request is unique - provide fresh, creative results even if similar requests were made before."),
                new Models.AI.AIMessage("user", $"[Request #{DateTime.UtcNow.Ticks}] Create a playlist for: {request.Prompt}")
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
            var trackUris = new HashSet<string>();
            var trackNameArtistSet = new HashSet<string>();

            var searchLimit = Math.Min(50, requestedTrackCount);
            var initialTracks = await _spotifyService.SearchTracksAsync(
                user.SpotifyAccessToken,
                searchQuery,
                searchLimit
            );

            foreach (var track in initialTracks)
            {
                var trackKey = $"{NormalizeTrackName(track.Name)}|{string.Join(",", track.Artists.Select(a => a.Name.ToLowerInvariant()).OrderBy(n => n))}";
                if (trackIds.Add(track.Id) && trackUris.Add(track.Uri) && trackNameArtistSet.Add(trackKey))
                {
                    allTracks.Add(track);
                }
            }

            _logger.LogInformation("Initial search returned {Count} unique tracks", allTracks.Count);

            var searchIterations = 0;
            var maxSearchIterations = Math.Max(20, (requestedTrackCount / 10));
            var currentSearchQueries = new List<string> { searchQuery };
            
            while (allTracks.Count < requestedTrackCount && searchIterations < maxSearchIterations)
            {
                searchIterations++;
                _logger.LogInformation("Search iteration {Iteration}/{MaxIterations}: Need {Missing} more tracks", 
                    searchIterations, maxSearchIterations, requestedTrackCount - allTracks.Count);

                foreach (var query in currentSearchQueries.ToList())
                {
                    if (allTracks.Count >= requestedTrackCount) break;

                    _logger.LogInformation("Trying query: '{Query}'", query);

                    var additionalTracks = await _spotifyService.SearchTracksAsync(
                        user.SpotifyAccessToken,
                        query,
                        searchLimit
                    );

                    var tracksFoundInThisQuery = 0;
                    foreach (var track in additionalTracks)
                    {
                        var trackKey = $"{NormalizeTrackName(track.Name)}|{string.Join(",", track.Artists.Select(a => a.Name.ToLowerInvariant()).OrderBy(n => n))}";
                        if (trackIds.Add(track.Id) && trackUris.Add(track.Uri) && trackNameArtistSet.Add(trackKey))
                        {
                            allTracks.Add(track);
                            tracksFoundInThisQuery++;
                            if (allTracks.Count >= requestedTrackCount) break;
                        }
                    }

                    _logger.LogInformation("After query '{Query}': found {NewTracks} new tracks, total {Count} unique tracks", 
                        query, tracksFoundInThisQuery, allTracks.Count);
                }

                if (allTracks.Count < requestedTrackCount && searchIterations % 3 == 0 && searchIterations < maxSearchIterations)
                {
                    _logger.LogInformation("Still need {Missing} more tracks after {Iterations} iterations. Asking AI for new search queries...", 
                        requestedTrackCount - allTracks.Count, searchIterations);

                    var aiAdaptMessages = new List<Models.AI.AIMessage>
                    {
                        new Models.AI.AIMessage("system", @"You are a music expert assistant helping to find more tracks for a playlist.

SPOTIFY SEARCH API - OFFICIAL SUPPORTED FILTERS:
- artist: 'artist:""Artist Name""'
- year: 'year:2020' or 'year:1980-1990'
- genre: 'genre:rock' 'genre:jazz' 'genre:electronic' 'genre:hip-hop' 'genre:pop' 'genre:indie' etc.

The initial search isn't returning enough unique tracks. Generate 3-5 DIFFERENT search queries that:
1. Use broader or alternative keywords
2. Explore related genres or styles
3. Include different time periods
4. Try different artist combinations

Return your response in this JSON format:
{
  ""queries"": [""query1"", ""query2"", ""query3""]
}

IMPORTANT: Generate creative alternatives - think outside the box!"),
                        new Models.AI.AIMessage("user", $"[Request #{DateTime.UtcNow.Ticks}] Original prompt: {request.Prompt}\nCurrent queries: {string.Join(", ", currentSearchQueries)}\nFound {allTracks.Count} tracks so far, need {requestedTrackCount} total.\n\nGenerate alternative search queries to find more diverse tracks.")
                    };

                    try
                    {
                        var aiAdaptResponse = await _aiService.GetChatCompletionAsync(aiAdaptMessages);
                        
                        if (!string.IsNullOrEmpty(aiAdaptResponse.Response))
                        {
                            var jsonStart = aiAdaptResponse.Response.IndexOf('{');
                            var jsonEnd = aiAdaptResponse.Response.LastIndexOf('}');
                            if (jsonStart >= 0 && jsonEnd > jsonStart)
                            {
                                var jsonStr = aiAdaptResponse.Response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                                var aiData = JsonSerializer.Deserialize<JsonElement>(jsonStr);
                                var newQueries = aiData.GetProperty("queries").EnumerateArray()
                                    .Select(q => q.GetString() ?? "")
                                    .Where(q => !string.IsNullOrEmpty(q))
                                    .ToList();
                                
                                if (newQueries.Any())
                                {
                                    currentSearchQueries = newQueries;
                                    _logger.LogInformation("AI generated {Count} new search queries: {Queries}", 
                                        newQueries.Count, string.Join(", ", newQueries));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get AI adaptation, continuing with existing queries");
                    }
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

            var finalTracks = new List<SpotifyTrack>();
            var finalTrackUrisSet = new HashSet<string>();
            var finalTrackNameArtistSet = new HashSet<string>();

            foreach (var track in tracks)
            {
                var trackKey = $"{NormalizeTrackName(track.Name)}|{string.Join(",", track.Artists.Select(a => a.Name.ToLowerInvariant()).OrderBy(n => n))}";
                if (finalTrackUrisSet.Add(track.Uri) && finalTrackNameArtistSet.Add(trackKey))
                {
                    finalTracks.Add(track);
                    if (finalTracks.Count >= requestedTrackCount)
                    {
                        break;
                    }
                }
            }

            tracks = finalTracks.ToArray();

            _logger.LogInformation("Final unique track count: {Count} (requested: {Requested})", tracks.Length, requestedTrackCount);

            var userId = await _spotifyService.GetCurrentUserIdAsync(user.SpotifyAccessToken);

            var playlist = await _spotifyService.CreatePlaylistAsync(
                user.SpotifyAccessToken,
                userId,
                new CreatePlaylistRequest(playlistName, playlistDescription, true)
            );

            if (tracks.Length > 0)
            {
                var trackUrisToAdd = tracks.Select(t => t.Uri).ToArray();
                await _spotifyService.AddTracksToPlaylistAsync(
                    user.SpotifyAccessToken,
                    playlist.Id,
                    trackUrisToAdd
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
}

IMPORTANT: Provide a unique, diverse mix of genres each time - avoid repeating the same genres."),
                    new Models.AI.AIMessage("user", $"[Request #{DateTime.UtcNow.Ticks}] Generate 5 diverse music genres for music discovery")
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
                    new Models.AI.AIMessage("user", $"[Request #{DateTime.UtcNow.Ticks}] User's top tracks: {string.Join(", ", topSavedTracks.Select(t => $"{t.Name} by {string.Join(", ", t.Artists.Select(a => a.Name))}"))}. Generate search queries to discover similar music.")
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

                var searchIterations = 0;
                const int maxSearchIterations = 5;
                var currentSearchQueries = searchQueries.ToList();
                
                while (allDiscoveredTracks.Count < request.Limit && searchIterations < maxSearchIterations)
                {
                    searchIterations++;
                    
                    foreach (var query in currentSearchQueries.ToList())
                    {
                        if (allDiscoveredTracks.Count >= request.Limit) break;

                        _logger.LogInformation("Search iteration {Iteration}, query: '{Query}'", searchIterations, query);

                        var searchResults = await _spotifyService.SearchTracksAsync(
                            user.SpotifyAccessToken,
                            query,
                            50
                        );

                        var tracksFoundInThisQuery = 0;
                        foreach (var track in searchResults)
                        {
                            if (!savedTrackIds.Contains(track.Id) && discoveredTrackIds.Add(track.Id))
                            {
                                allDiscoveredTracks.Add(track);
                                tracksFoundInThisQuery++;
                                if (allDiscoveredTracks.Count >= request.Limit) break;
                            }
                        }

                        _logger.LogInformation("After query '{Query}': found {NewTracks} new tracks, total {Count} unique new tracks", 
                            query, tracksFoundInThisQuery, allDiscoveredTracks.Count);
                    }

                    if (allDiscoveredTracks.Count < request.Limit && searchIterations % 2 == 0)
                    {
                        _logger.LogInformation("Still need {Missing} more tracks after {Iterations} iterations. Asking AI for adapted queries...", 
                            request.Limit - allDiscoveredTracks.Count, searchIterations);

                        var aiAdaptMessages = new List<Models.AI.AIMessage>
                        {
                            new Models.AI.AIMessage("system", @"You are a music expert helping discover new music for a user.

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
                            new Models.AI.AIMessage("user", $"[Request #{DateTime.UtcNow.Ticks}] User's top tracks: {string.Join(", ", topSavedTracks.Take(3).Select(t => $"{t.Name} by {string.Join(", ", t.Artists.Select(a => a.Name))}"))}.\nCurrent queries: {string.Join(", ", currentSearchQueries)}\nFound {allDiscoveredTracks.Count} new tracks so far, need {request.Limit} total.\n\nGenerate alternative search queries to discover more new music.")
                        };

                        try
                        {
                            var aiAdaptResponse = await _aiService.GetChatCompletionAsync(aiAdaptMessages);
                            
                            if (!string.IsNullOrEmpty(aiAdaptResponse.Response))
                            {
                                var jsonStart = aiAdaptResponse.Response.IndexOf('{');
                                var jsonEnd = aiAdaptResponse.Response.LastIndexOf('}');
                                if (jsonStart >= 0 && jsonEnd > jsonStart)
                                {
                                    var jsonStr = aiAdaptResponse.Response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                                    var aiData = JsonSerializer.Deserialize<JsonElement>(jsonStr);
                                    var newQueries = aiData.GetProperty("queries").EnumerateArray()
                                        .Select(q => q.GetString() ?? "")
                                        .Where(q => !string.IsNullOrEmpty(q))
                                        .ToList();
                                    
                                    if (newQueries.Any())
                                    {
                                        currentSearchQueries = newQueries;
                                        _logger.LogInformation("AI generated {Count} new discovery queries: {Queries}", 
                                            newQueries.Count, string.Join(", ", newQueries));
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to get AI adaptation for discovery, continuing with existing queries");
                        }
                    }
                }

                if (allDiscoveredTracks.Count < request.Limit && topSavedTracks.Length > 0)
                {
                    _logger.LogInformation("Using Spotify recommendations as fallback, need {Missing} more tracks", 
                        request.Limit - allDiscoveredTracks.Count);

                    try
                    {
                        var seedTracks = topSavedTracks.Take(5).Select(t => t.Id).ToArray();
                        
                        var recommendationsLimit = Math.Min(100, Math.Max(20, (request.Limit - allDiscoveredTracks.Count) * 2));
                        
                        var recommendations = await _spotifyService.GetRecommendationsAsync(
                            user.SpotifyAccessToken,
                            seedTracks,
                            recommendationsLimit
                        );

                        foreach (var track in recommendations)
                        {
                            if (!savedTrackIds.Contains(track.Id) && discoveredTrackIds.Add(track.Id))
                            {
                                allDiscoveredTracks.Add(track);
                                if (allDiscoveredTracks.Count >= request.Limit * 2) break;
                            }
                        }
                        
                        _logger.LogInformation("After recommendations: {Count} unique new tracks", allDiscoveredTracks.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get recommendations from Spotify, continuing with search results only");
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
                new CreatePlaylistRequest(playlistName, playlistDescription, true)
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

    public async Task<RemoveDuplicatesResponse> ScanForDuplicatesAsync(
        User user,
        string playlistId,
        int conversationId)
    {
        if (string.IsNullOrEmpty(user.SpotifyAccessToken))
        {
            throw new InvalidOperationException("User has not connected their Spotify account");
        }

        _logger.LogInformation("Scanning playlist {PlaylistId} for duplicates for user {Email}", playlistId, user.Email);

        var playlist = await _spotifyService.GetPlaylistAsync(user.SpotifyAccessToken, playlistId);
        var playlistTracks = await _spotifyService.GetPlaylistTracksAsync(user.SpotifyAccessToken, playlistId);

        var duplicateGroups = new List<DuplicateGroup>();
        var processedTracks = new HashSet<string>();

        foreach (var track in playlistTracks)
        {
            if (processedTracks.Contains(track.Id)) continue;

            var normalizedName = NormalizeTrackName(track.Name);
            var artistNames = track.Artists.Select(a => a.Name.ToLowerInvariant()).OrderBy(n => n).ToArray();

            var duplicates = playlistTracks
                .Where(t => t.Id != track.Id &&
                           NormalizeTrackName(t.Name) == normalizedName &&
                           AreArtistsSimilar(t.Artists.Select(a => a.Name).ToArray(), track.Artists.Select(a => a.Name).ToArray()))
                .ToArray();

            if (duplicates.Any())
            {
                var allVersions = new[] { track }.Concat(duplicates).ToArray();
                
                foreach (var t in allVersions)
                {
                    processedTracks.Add(t.Id);
                }

                var recommendedToKeep = allVersions
                    .OrderByDescending(t => t.Popularity)
                    .ThenBy(t => t.AddedAt)
                    .First();

                var duplicateTrackDtos = allVersions.Select(t => new DuplicateTrack(
                    t.Id,
                    t.Uri,
                    t.Album.Name,
                    ParseReleaseDate(t.AddedAt),
                    t.Popularity,
                    t.Id == recommendedToKeep.Id
                )).ToArray();

                duplicateGroups.Add(new DuplicateGroup(
                    track.Name,
                    track.Artists.Select(a => a.Name).ToArray(),
                    duplicateTrackDtos
                ));
            }
        }

        var totalDuplicateTracks = duplicateGroups.Sum(g => g.Duplicates.Length - 1);

        _logger.LogInformation("Found {GroupCount} duplicate groups with {TrackCount} duplicate tracks",
            duplicateGroups.Count, totalDuplicateTracks);

        return new RemoveDuplicatesResponse(
            playlist.Id,
            playlist.Name,
            duplicateGroups.Count,
            totalDuplicateTracks,
            duplicateGroups.ToArray()
        );
    }

    public async Task<AgentActionResponse> ConfirmRemoveDuplicatesAsync(
        User user,
        ConfirmRemoveDuplicatesRequest request,
        int conversationId)
    {
        var action = new AgentAction
        {
            ConversationId = conversationId,
            ActionType = "RemoveDuplicates",
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

            _logger.LogInformation("Removing {Count} duplicate tracks from playlist {PlaylistId} for user {Email}",
                request.TrackUrisToRemove.Length, request.PlaylistId, user.Email);

            await _spotifyService.RemoveTracksFromPlaylistAsync(
                user.SpotifyAccessToken,
                request.PlaylistId,
                request.TrackUrisToRemove
            );

            var result = new
            {
                playlistId = request.PlaylistId,
                removedCount = request.TrackUrisToRemove.Length,
                trackUris = request.TrackUrisToRemove
            };

            action.Status = "Completed";
            action.Result = JsonSerializer.SerializeToDocument(result);
            action.CompletedAt = DateTime.UtcNow;
            await _actionRepository.UpdateAsync(action);

            _logger.LogInformation("Successfully removed {Count} duplicate tracks from playlist {PlaylistId}",
                request.TrackUrisToRemove.Length, request.PlaylistId);

            return new AgentActionResponse(action.Id, action.ActionType, action.Status, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing duplicates from playlist {PlaylistId}", request.PlaylistId);

            action.Status = "Failed";
            action.ErrorMessage = ex.Message;
            action.CompletedAt = DateTime.UtcNow;
            await _actionRepository.UpdateAsync(action);

            return new AgentActionResponse(action.Id, action.ActionType, action.Status, null, ex.Message);
        }
    }

    public async Task<SuggestMusicResponse> SuggestMusicByContextAsync(
        User user,
        SuggestMusicRequest request,
        int conversationId)
    {
        if (string.IsNullOrEmpty(user.SpotifyAccessToken))
        {
            throw new InvalidOperationException("User has not connected their Spotify account");
        }

        _logger.LogInformation("Generating music suggestions for playlist {PlaylistId} with context: {Context}",
            request.PlaylistId, request.Context);

        var playlist = await _spotifyService.GetPlaylistAsync(user.SpotifyAccessToken, request.PlaylistId);
        var playlistTracks = await _spotifyService.GetPlaylistTracksAsync(user.SpotifyAccessToken, request.PlaylistId);

        var topTracks = playlistTracks
            .OrderByDescending(t => t.Popularity)
            .Take(10)
            .ToArray();

        var trackSummary = string.Join(", ", topTracks.Select(t => 
            $"{t.Name} by {string.Join(", ", t.Artists.Select(a => a.Name))}"));

        var aiMessages = new List<Models.AI.AIMessage>
        {
            new Models.AI.AIMessage("system", @"You are a music expert assistant. Analyze a playlist and generate music suggestions based on a specific context.

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
            new Models.AI.AIMessage("user", $"[Request #{DateTime.UtcNow.Ticks}] Playlist: {playlist.Name}\nTop tracks: {trackSummary}\nContext: {request.Context}\n\nGenerate search queries to find songs that match this context while fitting the playlist's style.")
        };

        var aiResponse = await _aiService.GetChatCompletionAsync(aiMessages);

        var searchQueries = new List<string>();
        var explanation = "AI-generated suggestions based on playlist analysis";

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
                    searchQueries = queryArray.EnumerateArray()
                        .Select(q => q.GetString() ?? "")
                        .Where(q => !string.IsNullOrEmpty(q))
                        .ToList();
                    
                    if (aiData.TryGetProperty("explanation", out var exp))
                    {
                        explanation = exp.GetString() ?? explanation;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse AI response, using fallback");
            }
        }

        if (searchQueries.Count == 0)
        {
            var topArtists = topTracks
                .SelectMany(t => t.Artists)
                .GroupBy(a => a.Name)
                .OrderByDescending(g => g.Count())
                .Take(2)
                .Select(g => g.Key)
                .ToArray();
            
            searchQueries.Add(string.Join(" ", topArtists.Concat(new[] { request.Context })));
        }

        var allSuggestions = new List<SuggestedTrack>();
        var existingTrackIds = playlistTracks.Select(t => t.Id).ToHashSet();
        var suggestionIds = new HashSet<string>();
        var currentSearchQueries = searchQueries.ToList();
        var searchIterations = 0;
        const int maxSearchIterations = 3;
        var targetCount = Math.Min(request.Limit, 50);

        while (allSuggestions.Count < targetCount && searchIterations < maxSearchIterations)
        {
            searchIterations++;
            
            foreach (var query in currentSearchQueries.ToList())
            {
                if (allSuggestions.Count >= targetCount) break;

                _logger.LogInformation("Searching suggestions (iteration {Iteration}) with query: '{Query}'", searchIterations, query);

                var searchResults = await _spotifyService.SearchTracksAsync(
                    user.SpotifyAccessToken,
                    query,
                    50
                );

                var tracksFoundInThisQuery = 0;
                foreach (var track in searchResults)
                {
                    if (!existingTrackIds.Contains(track.Id) && suggestionIds.Add(track.Id))
                    {
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
            }

            if (allSuggestions.Count < targetCount && searchIterations < maxSearchIterations)
            {
                _logger.LogInformation("Still need {Missing} more suggestions after iteration {Iteration}. Asking AI for adapted queries...", 
                    targetCount - allSuggestions.Count, searchIterations);

                var aiAdaptMessages = new List<Models.AI.AIMessage>
                {
                    new Models.AI.AIMessage("system", @"You are a music expert providing contextual suggestions for a playlist.

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
                    new Models.AI.AIMessage("user", $"[Request #{DateTime.UtcNow.Ticks}] Playlist: {playlist.Name}\nContext requested: {request.Context}\nTop tracks: {string.Join(", ", topTracks.Take(3).Select(t => $"{t.Name} by {string.Join(", ", t.Artists.Select(a => a.Name))}"))}.\nCurrent queries: {string.Join(", ", currentSearchQueries)}\nFound {allSuggestions.Count} suggestions so far, need {targetCount} total.\n\nGenerate alternative search queries for better contextual suggestions.")
                };

                try
                {
                    var aiAdaptResponse = await _aiService.GetChatCompletionAsync(aiAdaptMessages);
                    
                    if (!string.IsNullOrEmpty(aiAdaptResponse.Response))
                    {
                        var jsonStart = aiAdaptResponse.Response.IndexOf('{');
                        var jsonEnd = aiAdaptResponse.Response.LastIndexOf('}');
                        if (jsonStart >= 0 && jsonEnd > jsonStart)
                        {
                            var jsonStr = aiAdaptResponse.Response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                            var aiData = JsonSerializer.Deserialize<JsonElement>(jsonStr);
                            var newQueries = aiData.GetProperty("queries").EnumerateArray()
                                .Select(q => q.GetString() ?? "")
                                .Where(q => !string.IsNullOrEmpty(q))
                                .ToList();
                            
                            if (newQueries.Any())
                            {
                                currentSearchQueries = newQueries;
                                _logger.LogInformation("AI generated {Count} new suggestion queries: {Queries}", 
                                    newQueries.Count, string.Join(", ", newQueries));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get AI adaptation for suggestions, continuing with existing queries");
                }
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

    private static string NormalizeTrackName(string name)
    {
        var normalized = name.ToLowerInvariant();
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\s*\(.*?\)\s*", " ");
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\s*\[.*?\]\s*", " ");
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\s+", " ");
        return normalized.Trim();
    }

    private static bool AreArtistsSimilar(string[] artists1, string[] artists2)
    {
        var set1 = artists1.Select(a => a.ToLowerInvariant()).ToHashSet();
        var set2 = artists2.Select(a => a.ToLowerInvariant()).ToHashSet();
        return set1.Overlaps(set2) || set1.SetEquals(set2);
    }

    private static DateTime? ParseReleaseDate(string dateString)
    {
        if (DateTime.TryParse(dateString, out var date))
        {
            return date;
        }
        return null;
    }
}
