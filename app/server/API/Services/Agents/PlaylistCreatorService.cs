using System.Text.Json;
using API.DTOs.Agent;
using API.DTOs.Spotify;
using API.Interfaces;
using API.Models;
using API.Repositories;
using API.Services.Helpers;

namespace API.Services.Agents;

public class PlaylistCreatorService : IPlaylistCreatorService {
    private readonly ISpotifyService _spotifyService;
    private readonly ISpotifyUserService _userService;
    private readonly ISpotifyPlaylistService _playlistService;
    private readonly ISpotifyTokenService _tokenService;
    private readonly IAIService _aiService;
    private readonly IAIPromptBuilder _promptBuilder;
    private readonly IAIResponseParser _responseParser;
    private readonly TrackFilterHelper _trackFilterHelper;
    private readonly IAgentActionRepository _actionRepository;
    private readonly IAgentNotificationService _notificationService;
    private readonly ILogger<PlaylistCreatorService> _logger;

    public PlaylistCreatorService(
        ISpotifyService spotifyService,
        ISpotifyUserService userService,
        ISpotifyPlaylistService playlistService,
        ISpotifyTokenService tokenService,
        IAIService aiService,
        IAIPromptBuilder promptBuilder,
        IAIResponseParser responseParser,
        IAgentActionRepository actionRepository,
        IAgentNotificationService notificationService,
        ILogger<PlaylistCreatorService> logger) {
        _spotifyService = spotifyService;
        _userService = userService;
        _playlistService = playlistService;
        _tokenService = tokenService;
        _aiService = aiService;
        _promptBuilder = promptBuilder;
        _responseParser = responseParser;
        _trackFilterHelper = new TrackFilterHelper(spotifyService);
        _actionRepository = actionRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<AgentActionResponse> CreateSmartPlaylistAsync(
        User user,
        CreateSmartPlaylistRequest request,
        int conversationId) {
        var action = new AgentAction {
            ConversationId = conversationId,
            ActionType = "CreateSmartPlaylist",
            Status = "Processing",
            InputPrompt = request.Prompt,
            Parameters = JsonSerializer.SerializeToDocument(request),
            CreatedAt = DateTime.UtcNow
        };

        action = await _actionRepository.CreateAsync(action);

        try {
            await _notificationService.SendStatusUpdateAsync(user.Email, "processing", "Analyzing your request with AI...");

            var accessToken = await _tokenService.GetValidAccessTokenAsync(user);

            _logger.LogInformation("Creating smart playlist for user {Email} with prompt: {Prompt}",
                user.Email, request.Prompt);

            var aiMessages = _promptBuilder.BuildPlaylistCreationPrompt(request.Prompt);
            var aiResponse = await _aiService.GetChatCompletionAsync(aiMessages);

            var (playlistName, searchQuery, playlistDescription) = _responseParser.ParsePlaylistResponse(
                aiResponse.Response,
                request.Prompt
            );

            _logger.LogInformation("Using playlist name: {PlaylistName}, search query: {SearchQuery}", playlistName, searchQuery);

            await _notificationService.SendStatusUpdateAsync(user.Email, "processing", $"Searching Spotify for tracks matching: {playlistName}");

            var requestedTrackCount = request.Preferences?.MaxTracks ?? 20;
            var allTracks = new List<SpotifyTrack>();
            var trackIds = new HashSet<string>();
            var trackUris = new HashSet<string>();
            var trackNameArtistSet = new HashSet<string>();

            var searchLimit = Math.Min(50, requestedTrackCount);
            var initialTracks = await _spotifyService.SearchTracksAsync(
                accessToken,
                searchQuery,
                searchLimit
            );

            foreach (var track in initialTracks) {
                var trackKey = TrackDeduplicationHelper.GetTrackKey(track);
                bool hasSimilarTitle = TrackDeduplicationHelper.IsDuplicateTrack(track, allTracks);

                if (!hasSimilarTitle && trackIds.Add(track.Id) && trackUris.Add(track.Uri) && trackNameArtistSet.Add(trackKey)) {
                    allTracks.Add(track);
                }
            }

            _logger.LogInformation("Initial search returned {Count} unique tracks", allTracks.Count);

            await _notificationService.SendStatusUpdateAsync(user.Email, "processing", $"Found {allTracks.Count} tracks in initial search. Searching for more...");

            var searchIterations = 0;
            var maxSearchIterations = Math.Max(20, (requestedTrackCount / 10));
            var currentSearchQueries = new List<string> { searchQuery };

            while (allTracks.Count < requestedTrackCount && searchIterations < maxSearchIterations) {
                searchIterations++;
                _logger.LogInformation("Search iteration {Iteration}/{MaxIterations}: Need {Missing} more tracks",
                    searchIterations, maxSearchIterations, requestedTrackCount - allTracks.Count);

                await _notificationService.SendStatusUpdateAsync(user.Email, "processing", $"Iteration {searchIterations}/{maxSearchIterations}: Found {allTracks.Count}/{requestedTrackCount} tracks");

                foreach (var query in currentSearchQueries.ToList()) {
                    if (allTracks.Count >= requestedTrackCount) break;

                    _logger.LogInformation("Trying query: '{Query}'", query);
                    await _notificationService.SendStatusUpdateAsync(user.Email, "processing", $"Searching with query: '{query}'");

                    var additionalTracks = await _spotifyService.SearchTracksAsync(
                        accessToken,
                        query,
                        searchLimit
                    );

                    var tracksFoundInThisQuery = 0;
                    foreach (var track in additionalTracks) {
                        var trackKey = TrackDeduplicationHelper.GetTrackKey(track);
                        bool hasSimilarTitle = TrackDeduplicationHelper.IsDuplicateTrack(track, allTracks);

                        if (!hasSimilarTitle && trackIds.Add(track.Id) && trackUris.Add(track.Uri) && trackNameArtistSet.Add(trackKey)) {
                            allTracks.Add(track);
                            tracksFoundInThisQuery++;
                            if (allTracks.Count >= requestedTrackCount) break;
                        }
                    }

                    _logger.LogInformation("After query '{Query}': found {NewTracks} new tracks, total {Count} unique tracks",
                        query, tracksFoundInThisQuery, allTracks.Count);

                    await _notificationService.SendStatusUpdateAsync(user.Email, "processing", $"After query '{query}': found {tracksFoundInThisQuery} new tracks, total {allTracks.Count} unique tracks");
                }

                if (allTracks.Count < requestedTrackCount && searchIterations % 3 == 0 && searchIterations < maxSearchIterations) {
                    _logger.LogInformation("Still need {Missing} more tracks after {Iterations} iterations. Asking AI for new search queries...",
                        requestedTrackCount - allTracks.Count, searchIterations);

                    var aiAdaptMessages = _promptBuilder.BuildAdaptiveSearchPrompt(
                        request.Prompt,
                        currentSearchQueries.ToArray(),
                        allTracks.Count,
                        requestedTrackCount
                    );

                    try {
                        var aiAdaptResponse = await _aiService.GetChatCompletionAsync(aiAdaptMessages);
                        var newQueries = _responseParser.ParseQueriesResponse(aiAdaptResponse.Response);

                        if (newQueries.Length > 0) {
                            currentSearchQueries = newQueries.ToList();
                            _logger.LogInformation("AI generated {Count} new search queries: {Queries}",
                                newQueries.Length, string.Join(", ", newQueries));

                            await _notificationService.SendStatusUpdateAsync(user.Email, "processing", $"AI generated {newQueries.Length} new search strategies to find more tracks");
                        }
                    } catch (Exception ex) {
                        _logger.LogWarning(ex, "Failed to get AI adaptation, continuing with existing queries");
                    }
                }
            }

            var tracks = allTracks.ToArray();

            if (request.Preferences != null) {
                tracks = await _trackFilterHelper.FilterByPreferencesAsync(
                    accessToken,
                    tracks,
                    request.Preferences
                );

                _logger.LogInformation("After filtering by preferences: {Count} tracks", tracks.Length);
            }

            var finalTracks = new List<SpotifyTrack>();
            var finalTrackUrisSet = new HashSet<string>();
            var finalTrackNameArtistSet = new HashSet<string>();

            foreach (var track in tracks) {
                var trackKey = TrackDeduplicationHelper.GetTrackKey(track);
                if (finalTrackUrisSet.Add(track.Uri) && finalTrackNameArtistSet.Add(trackKey)) {
                    finalTracks.Add(track);
                    if (finalTracks.Count >= requestedTrackCount) {
                        break;
                    }
                }
            }

            tracks = finalTracks.ToArray();

            _logger.LogInformation("Final unique track count: {Count} (requested: {Requested})", tracks.Length, requestedTrackCount);

            await _notificationService.SendStatusUpdateAsync(user.Email, "processing", $"Creating playlist with {tracks.Length} tracks...");

            var userId = await _userService.GetCurrentUserIdAsync(accessToken);

            var playlist = await _playlistService.CreatePlaylistAsync(
                accessToken,
                userId,
                new CreatePlaylistRequest(playlistName, playlistDescription, true)
            );

            if (tracks.Length > 0) {
                await _notificationService.SendStatusUpdateAsync(user.Email, "processing", "Adding tracks to playlist...");
                var trackUrisToAdd = tracks.Select(t => t.Uri).ToArray();
                await _playlistService.AddTracksToPlaylistAsync(
                    accessToken,
                    playlist.Id,
                    trackUrisToAdd
                );
            }

            var result = new {
                playlistId = playlist.Id,
                playlistName = playlist.Name,
                playlistUri = playlist.Uri,
                trackCount = tracks.Length,
                tracks = tracks.Select(t => new {
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

            await _notificationService.SendStatusUpdateAsync(user.Email, "completed", $"Playlist '{playlist.Name}' created successfully!", new { playlistId = playlist.Id, trackCount = tracks.Length });

            _logger.LogInformation("Successfully created playlist {PlaylistId} with {TrackCount} tracks",
                playlist.Id, tracks.Length);

            return new AgentActionResponse(action.Id, action.ActionType, action.Status, result);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error creating smart playlist for user {Email}", user.Email);

            await _notificationService.SendStatusUpdateAsync(user.Email, "error", $"Failed to create playlist: {ex.Message}");

            action.Status = "Failed";
            action.ErrorMessage = ex.Message;
            action.CompletedAt = DateTime.UtcNow;
            await _actionRepository.UpdateAsync(action);

            return new AgentActionResponse(action.Id, action.ActionType, action.Status, null, ex.Message);
        }
    }
}