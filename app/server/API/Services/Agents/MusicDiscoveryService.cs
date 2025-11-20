using System.Text.Json;
using API.DTOs.Agent;
using API.DTOs.Spotify;
using API.Interfaces;
using API.Models;
using API.Repositories;

namespace API.Services.Agents;

public class MusicDiscoveryService : IMusicDiscoveryService {
    private readonly ISpotifyService _spotifyService;
    private readonly ISpotifyUserService _userService;
    private readonly ISpotifyPlaylistService _playlistService;
    private readonly ISpotifyTokenService _tokenService;
    private readonly IAgentActionRepository _actionRepository;
    private readonly IAgentNotificationService _notificationService;
    private readonly IDiscoveryQueryGenerator _queryGenerator;
    private readonly ITrackDiscoveryHelper _trackDiscoveryHelper;
    private readonly ILogger<MusicDiscoveryService> _logger;

    public MusicDiscoveryService(
        ISpotifyService spotifyService,
        ISpotifyUserService userService,
        ISpotifyPlaylistService playlistService,
        ISpotifyTokenService tokenService,
        IAgentActionRepository actionRepository,
        IAgentNotificationService notificationService,
        IDiscoveryQueryGenerator queryGenerator,
        ITrackDiscoveryHelper trackDiscoveryHelper,
        ILogger<MusicDiscoveryService> logger) {
        _spotifyService = spotifyService;
        _userService = userService;
        _playlistService = playlistService;
        _tokenService = tokenService;
        _actionRepository = actionRepository;
        _notificationService = notificationService;
        _queryGenerator = queryGenerator;
        _trackDiscoveryHelper = trackDiscoveryHelper;
        _logger = logger;
    }

    public async Task<AgentActionResponse> DiscoverNewMusicAsync(
        User user,
        DiscoverNewMusicRequest request,
        int conversationId) {
        var action = new AgentAction {
            ConversationId = conversationId,
            ActionType = "DiscoverNewMusic",
            Status = "Processing",
            Parameters = JsonSerializer.SerializeToDocument(request),
            CreatedAt = DateTime.UtcNow
        };

        action = await _actionRepository.CreateAsync(action);

        try {
            await _notificationService.SendStatusUpdateAsync(user.Email, "processing", "Analyzing your music taste...");

            var accessToken = await _tokenService.GetValidAccessTokenAsync(user);

            _logger.LogInformation("Discovering new music for user {Email}, requested: {Limit} tracks", user.Email, request.Limit);

            var savedTracks = await _spotifyService.GetUserSavedTracksAsync(accessToken, 50);
            var savedTrackIds = savedTracks.Select(t => t.Id).ToHashSet();

            _logger.LogInformation("User has {Count} saved tracks", savedTrackIds.Count);

            await _notificationService.SendStatusUpdateAsync(user.Email, "processing", "Checking your playlists to avoid duplicates...");

            var userPlaylists = await _spotifyService.GetUserPlaylistsAsync(accessToken, 50);
            _logger.LogInformation("User has {Count} playlists", userPlaylists.Length);

            foreach (var userPlaylist in userPlaylists) {
                try {
                    var playlistTracks = await _spotifyService.GetPlaylistTracksAsync(accessToken, userPlaylist.Id);
                    foreach (var track in playlistTracks) {
                        savedTrackIds.Add(track.Id);
                    }
                } catch (Exception ex) {
                    _logger.LogWarning(ex, "Failed to get tracks from playlist {PlaylistId}", userPlaylist.Id);
                }
            }

            _logger.LogInformation("Total unique tracks across saved songs and playlists: {Count}", savedTrackIds.Count);

            var topSavedTracks = savedTracks.OrderByDescending(t => t.Popularity).Take(5).ToArray();
            List<SpotifyTrack> allDiscoveredTracks = new List<SpotifyTrack>();
            var discoveryAttempts = 0;
            const int maxDiscoveryAttempts = 5;

            while (allDiscoveredTracks.Count < request.Limit && discoveryAttempts < maxDiscoveryAttempts) {
                discoveryAttempts++;
                var remainingNeeded = request.Limit - allDiscoveredTracks.Count;
                
                _logger.LogInformation("Discovery attempt {Attempt}: Need {Remaining} more tracks (have {Current})", 
                    discoveryAttempts, remainingNeeded, allDiscoveredTracks.Count);
                
                await _notificationService.SendStatusUpdateAsync(user.Email, "processing", 
                    $"Discovery attempt {discoveryAttempts}: Looking for {remainingNeeded} more unique tracks...");

                List<SpotifyTrack> newDiscoveredTracks;

                if (topSavedTracks.Length == 0) {
                    newDiscoveredTracks = await _trackDiscoveryHelper.DiscoverWithoutSavedTracksAsync(
                        accessToken, remainingNeeded, savedTrackIds);
                } else {
                    var searchQueries = await _queryGenerator.GenerateQueriesAsync(topSavedTracks);
                    _logger.LogInformation("Using {Count} search queries for discovery", searchQueries.Count);

                    newDiscoveredTracks = await _trackDiscoveryHelper.DiscoverFromSearchQueriesAsync(
                        accessToken, searchQueries, remainingNeeded, savedTrackIds, user.Email);

                    if (newDiscoveredTracks.Count < remainingNeeded) {
                        newDiscoveredTracks = await _trackDiscoveryHelper.FallbackToRecommendationsAsync(
                            accessToken, topSavedTracks, remainingNeeded, newDiscoveredTracks, savedTrackIds);
                    }
                }

                var uniqueNewTracks = 0;
                foreach (var track in newDiscoveredTracks) {
                    if (savedTrackIds.Add(track.Id)) {
                        allDiscoveredTracks.Add(track);
                        uniqueNewTracks++;
                        if (allDiscoveredTracks.Count >= request.Limit) break;
                    }
                }

                _logger.LogInformation("Discovery attempt {Attempt}: Found {Unique} unique new tracks, total: {Total}", 
                    discoveryAttempts, uniqueNewTracks, allDiscoveredTracks.Count);

                if (uniqueNewTracks == 0) {
                    _logger.LogWarning("No new unique tracks found in attempt {Attempt}, stopping discovery", discoveryAttempts);
                    break;
                }
            }

            var newTracks = allDiscoveredTracks.Take(request.Limit).ToArray();

            _logger.LogInformation("Final discovery: {Count} unique new tracks (requested: {Requested})", newTracks.Length, request.Limit);

            var userId = await _userService.GetCurrentUserIdAsync(accessToken);
            var playlistName = $"Discover Weekly - {DateTime.UtcNow:MMM dd, yyyy}";
            var playlistDescription = "AI-generated music discovery based on your listening habits";

            var playlist = await _playlistService.CreatePlaylistAsync(
                accessToken,
                userId,
                new CreatePlaylistRequest(playlistName, playlistDescription, true)
            );

            if (newTracks.Length > 0) {
                var trackUris = newTracks.Select(t => t.Uri).ToArray();
                await _playlistService.AddTracksToPlaylistAsync(accessToken, playlist.Id, trackUris);
            }

            var result = new {
                playlistId = playlist.Id,
                playlistName = playlist.Name,
                playlistUri = playlist.Uri,
                trackCount = newTracks.Length,
                tracks = newTracks.Select(t => new {
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
        } catch (Exception ex) {
            _logger.LogError(ex, "Error discovering new music for user {Email}", user.Email);

            action.Status = "Failed";
            action.ErrorMessage = ex.Message;
            action.CompletedAt = DateTime.UtcNow;
            await _actionRepository.UpdateAsync(action);

            return new AgentActionResponse(action.Id, action.ActionType, action.Status, null, ex.Message);
        }
    }
}