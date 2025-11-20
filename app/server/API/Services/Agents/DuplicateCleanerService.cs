using System.Text.Json;
using API.DTOs.Agent;
using API.Interfaces;
using API.Models;
using API.Repositories;
using API.Services;
using API.Services.Helpers;

namespace API.Services.Agents;

public class DuplicateCleanerService : IDuplicateCleanerService {
    private readonly IAgentActionRepository _actionRepository;
    private readonly ISpotifyTrackService _trackService;
    private readonly ISpotifyPlaylistService _playlistService;
    private readonly ISpotifyTokenService _tokenService;
    private readonly IAgentNotificationService _notificationService;
    private readonly ILogger<DuplicateCleanerService> _logger;

    public DuplicateCleanerService(
        IAgentActionRepository actionRepository,
        ISpotifyTrackService trackService,
        ISpotifyPlaylistService playlistService,
        ISpotifyTokenService tokenService,
        IAgentNotificationService notificationService,
        ILogger<DuplicateCleanerService> logger) {
        _actionRepository = actionRepository;
        _trackService = trackService;
        _playlistService = playlistService;
        _tokenService = tokenService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<RemoveDuplicatesResponse> ScanForDuplicatesAsync(
        User user,
        string playlistId,
        int conversationId) {
        var action = new AgentAction {
            ConversationId = conversationId,
            ActionType = "ScanDuplicates",
            Status = "Processing",
            Parameters = JsonSerializer.SerializeToDocument(new { playlistId }),
            CreatedAt = DateTime.UtcNow
        };

        action = await _actionRepository.CreateAsync(action);

        try {
            var accessToken = await _tokenService.GetValidAccessTokenAsync(user);

            _logger.LogInformation("Scanning playlist {PlaylistId} for duplicates for user {Email}", playlistId, user.Email);
            await _notificationService.SendStatusUpdateAsync(user.Email, "processing", "Fetching playlist tracks...");

            var playlist = await _playlistService.GetPlaylistAsync(accessToken, playlistId);
            var playlistTracks = await _trackService.GetPlaylistTracksAsync(accessToken, playlistId);

            await _notificationService.SendStatusUpdateAsync(user.Email, "processing", 
                $"Analyzing {playlistTracks.Length} tracks from '{playlist.Name}' for duplicates...");

            var duplicateGroups = new List<DuplicateGroup>();
            var processedTracks = new HashSet<string>();
            var tracksProcessed = 0;
            var lastProgressUpdate = 0;

            foreach (var track in playlistTracks) {
                tracksProcessed++;
                var progressPercent = (tracksProcessed * 100) / playlistTracks.Length;
                
                if (progressPercent - lastProgressUpdate >= 20) {
                    lastProgressUpdate = progressPercent;
                    await _notificationService.SendStatusUpdateAsync(user.Email, "processing", 
                        $"Scanning progress: {progressPercent}% ({tracksProcessed}/{playlistTracks.Length} tracks)");
                }

                if (processedTracks.Contains(track.Id)) continue;

                var normalizedName = TrackNormalizationHelper.NormalizeTrackName(track.Name);

                var duplicates = playlistTracks
                    .Where(t => t.Id != track.Id &&
                               TrackNormalizationHelper.NormalizeTrackName(t.Name) == normalizedName &&
                               TrackNormalizationHelper.AreArtistsSimilar(
                                   t.Artists.Select(a => a.Name).ToArray(),
                                   track.Artists.Select(a => a.Name).ToArray()))
                    .ToArray();

                if (duplicates.Any()) {
                    var allVersions = new[] { track }.Concat(duplicates).ToArray();

                    foreach (var t in allVersions) {
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
                        TrackDeduplicationHelper.ParseReleaseDate(t.AddedAt),
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

            if (duplicateGroups.Count > 0) {
                await _notificationService.SendStatusUpdateAsync(user.Email, "processing", 
                    $"Found {duplicateGroups.Count} duplicate groups with {totalDuplicateTracks} duplicate tracks to review");
            } else {
                await _notificationService.SendStatusUpdateAsync(user.Email, "processing", 
                    "No duplicates found in this playlist!");
            }

            var result = new {
                playlistId = playlist.Id,
                playlistName = playlist.Name,
                duplicateGroups = duplicateGroups.Count,
                totalDuplicates = totalDuplicateTracks
            };

            action.Status = "Completed";
            action.Result = JsonSerializer.SerializeToDocument(result);
            action.CompletedAt = DateTime.UtcNow;
            await _actionRepository.UpdateAsync(action);

            await _notificationService.SendStatusUpdateAsync(user.Email, "completed", 
                $"Scan complete for '{playlist.Name}'!", 
                new { duplicateGroups = duplicateGroups.Count, totalDuplicates = totalDuplicateTracks });

            return new RemoveDuplicatesResponse(
                playlist.Id,
                playlist.Name,
                duplicateGroups.Count,
                totalDuplicateTracks,
                duplicateGroups.ToArray()
            );
        } catch (Exception ex) {
            _logger.LogError(ex, "Error scanning for duplicates in playlist {PlaylistId}", playlistId);

            action.Status = "Failed";
            action.ErrorMessage = ex.Message;
            action.CompletedAt = DateTime.UtcNow;
            await _actionRepository.UpdateAsync(action);

            await _notificationService.SendStatusUpdateAsync(user.Email, "error", 
                $"Failed to scan for duplicates: {ex.Message}");

            throw;
        }
    }

    public async Task<AgentActionResponse> ConfirmRemoveDuplicatesAsync(
        User user,
        ConfirmRemoveDuplicatesRequest request,
        int conversationId) {
        var action = new AgentAction {
            ConversationId = conversationId,
            ActionType = "RemoveDuplicates",
            Status = "Processing",
            Parameters = JsonSerializer.SerializeToDocument(request),
            CreatedAt = DateTime.UtcNow
        };

        action = await _actionRepository.CreateAsync(action);

        try {
            var accessToken = await _tokenService.GetValidAccessTokenAsync(user);

            _logger.LogInformation("Removing {Count} duplicate tracks from playlist {PlaylistId} for user {Email}",
                request.TrackUrisToRemove.Length, request.PlaylistId, user.Email);

            await _notificationService.SendStatusUpdateAsync(user.Email, "processing", 
                $"Removing {request.TrackUrisToRemove.Length} duplicate tracks from playlist...");

            await _playlistService.RemoveTracksFromPlaylistAsync(
                accessToken,
                request.PlaylistId,
                request.TrackUrisToRemove
            );

            var result = new {
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

            await _notificationService.SendStatusUpdateAsync(user.Email, "completed", 
                $"Successfully removed {request.TrackUrisToRemove.Length} duplicate tracks!", 
                new { removedCount = request.TrackUrisToRemove.Length });

            return new AgentActionResponse(action.Id, action.ActionType, action.Status, result);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error removing duplicates from playlist {PlaylistId}", request.PlaylistId);

            action.Status = "Failed";
            action.ErrorMessage = ex.Message;
            action.CompletedAt = DateTime.UtcNow;
            await _actionRepository.UpdateAsync(action);

            await _notificationService.SendStatusUpdateAsync(user.Email, "error", 
                $"Failed to remove duplicates: {ex.Message}");

            return new AgentActionResponse(action.Id, action.ActionType, action.Status, null, ex.Message);
        }
    }
}