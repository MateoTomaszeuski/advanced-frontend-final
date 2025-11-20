using API.DTOs.Agent;
using API.Extensions;
using API.Interfaces;
using API.Repositories;
using API.Services;
using API.Services.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgentController : ControllerBase {
    private readonly IAgentService _agentService;
    private readonly IAgentActionRepository _actionRepository;
    private readonly IAgentAnalyticsService _analyticsService;
    private readonly IAgentHistoryService _historyService;
    private readonly ConversationValidator _conversationValidator;
    private readonly ILogger<AgentController> _logger;

    public AgentController(
        IConversationRepository conversationRepository,
        IAgentActionRepository actionRepository,
        IAgentService agentService,
        IAgentAnalyticsService analyticsService,
        IAgentHistoryService historyService,
        ILogger<AgentController> logger) {
        _agentService = agentService;
        _actionRepository = actionRepository;
        _analyticsService = analyticsService;
        _historyService = historyService;
        _conversationValidator = new ConversationValidator(conversationRepository);
        _logger = logger;
    }

    private async Task<(bool isValid, IActionResult? errorResult)> ValidateConversationAccessAsync(
        int conversationId,
        int userId) {
        var (isValid, _) = await _conversationValidator.ValidateUserOwnsConversationAsync(conversationId, userId);
        if (!isValid) {
            return (false, BadRequest(new { error = "Invalid conversation" }));
        }
        return (true, null);
    }

    [HttpPost("create-smart-playlist")]
    public async Task<IActionResult> CreateSmartPlaylist([FromBody] CreateSmartPlaylistWithConversationRequest request) {
        var user = this.GetCurrentUser();
        if (user == null) return this.UnauthorizedUser();

        _logger.LogInformation("CreateSmartPlaylist request from user: {Email}", user.Email);

        var (isValid, errorResult) = await ValidateConversationAccessAsync(request.ConversationId, user.Id);
        if (!isValid) return errorResult!;

        var result = await _agentService.CreateSmartPlaylistAsync(
            user,
            new CreateSmartPlaylistRequest(request.Prompt, request.Preferences),
            request.ConversationId
        );

        if (result.Status == "Failed") {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("discover-new-music")]
    public async Task<IActionResult> DiscoverNewMusic([FromBody] DiscoverNewMusicWithConversationRequest request) {
        var user = this.GetCurrentUser();
        if (user == null) return this.UnauthorizedUser();

        _logger.LogInformation("DiscoverNewMusic request from user: {Email}", user.Email);

        var (isValid, errorResult) = await ValidateConversationAccessAsync(request.ConversationId, user.Id);
        if (!isValid) return errorResult!;

        var result = await _agentService.DiscoverNewMusicAsync(
            user,
            new DiscoverNewMusicRequest(request.Limit, request.SourcePlaylistIds),
            request.ConversationId
        );

        if (result.Status == "Failed") {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("actions/{actionId}")]
    public async Task<IActionResult> GetAction(int actionId) {
        var user = this.GetCurrentUser();
        if (user == null) return this.UnauthorizedUser();

        _logger.LogInformation("GetAction request for action {ActionId} from user: {Email}", actionId, user.Email);

        var (action, isOwner) = await _historyService.GetActionByIdAsync(actionId, user.Id);

        if (action == null) {
            return NotFound(new { error = "Action not found" });
        }

        if (!isOwner) {
            _logger.LogWarning("User {Email} attempted to access action {ActionId} from another user",
                user.Email, actionId);
            return Forbid();
        }

        return Ok(action);
    }

    [HttpPost("scan-duplicates")]
    public async Task<IActionResult> ScanDuplicates([FromBody] ScanDuplicatesWithConversationRequest request) {
        var user = this.GetCurrentUser();
        if (user == null) return this.UnauthorizedUser();

        _logger.LogInformation("ScanDuplicates request from user: {Email} for playlist: {PlaylistId}",
            user.Email, request.PlaylistId);

        var (isValid, errorResult) = await ValidateConversationAccessAsync(request.ConversationId, user.Id);
        if (!isValid) return errorResult!;

        try {
            var result = await _agentService.ScanForDuplicatesAsync(
                user,
                request.PlaylistId,
                request.ConversationId
            );

            return Ok(result);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error scanning for duplicates");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("confirm-remove-duplicates")]
    public async Task<IActionResult> ConfirmRemoveDuplicates([FromBody] ConfirmRemoveDuplicatesWithConversationRequest request) {
        var user = this.GetCurrentUser();
        if (user == null) return this.UnauthorizedUser();

        _logger.LogInformation("ConfirmRemoveDuplicates request from user: {Email}", user.Email);

        var (isValid, errorResult) = await ValidateConversationAccessAsync(request.ConversationId, user.Id);
        if (!isValid) return errorResult!;

        var result = await _agentService.ConfirmRemoveDuplicatesAsync(
            user,
            new ConfirmRemoveDuplicatesRequest(request.PlaylistId, request.TrackUrisToRemove),
            request.ConversationId
        );

        if (result.Status == "Failed") {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("suggest-music")]
    public async Task<IActionResult> SuggestMusic([FromBody] SuggestMusicWithConversationRequest request) {
        var user = this.GetCurrentUser();
        if (user == null) return this.UnauthorizedUser();

        _logger.LogInformation("SuggestMusic request from user: {Email} for playlist: {PlaylistId}",
            user.Email, request.PlaylistId);

        var (isValid, errorResult) = await ValidateConversationAccessAsync(request.ConversationId, user.Id);
        if (!isValid) return errorResult!;

        try {
            var result = await _agentService.SuggestMusicByContextAsync(
                user,
                new SuggestMusicRequest(request.PlaylistId, request.Context, request.Limit),
                request.ConversationId
            );

            return Ok(result);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error suggesting music");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("recent-playlists")]
    public async Task<IActionResult> GetRecentlyCreatedPlaylists([FromQuery] int limit = 10) {
        var user = this.GetCurrentUser();
        if (user == null) {
            return this.UnauthorizedUser();
        }

        _logger.LogInformation("GetRecentlyCreatedPlaylists request from user: {Email}", user.Email);

        try {
            var result = await _historyService.GetRecentPlaylistsAsync(user.Id, limit);
            return Ok(result);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error getting recent playlists");
            return StatusCode(500, new { error = "Failed to fetch recent playlists" });
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] string? actionType = null, [FromQuery] string? status = null, [FromQuery] int limit = 50) {
        var user = this.GetCurrentUser();
        if (user == null) {
            return this.UnauthorizedUser();
        }

        _logger.LogInformation("GetHistory request for user: {Email}", user.Email);

        try {
            var result = await _historyService.GetHistoryAsync(user.Id, actionType, status, limit);
            return Ok(result);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error getting action history");
            return StatusCode(500, new { error = "Failed to get recent playlists" });
        }
    }

    [HttpDelete("history")]
    public async Task<IActionResult> ClearHistory() {
        var user = this.GetCurrentUser();
        if (user == null) {
            return this.UnauthorizedUser();
        }

        _logger.LogInformation("ClearHistory request from user: {Email}", user.Email);

        try {
            var deletedCount = await _historyService.ClearHistoryAsync(user.Id);
            _logger.LogInformation("Deleted {Count} actions for user: {Email}", deletedCount, user.Email);
            return Ok(new { message = $"Deleted {deletedCount} actions", deletedCount });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error clearing history for user: {Email}", user.Email);
            return StatusCode(500, new { error = "Failed to clear history" });
        }
    }

    [HttpGet("analytics")]
    public async Task<IActionResult> GetAppAnalytics() {
        var user = this.GetCurrentUser();
        if (user == null) {
            return this.UnauthorizedUser();
        }

        _logger.LogInformation("GetAppAnalytics request from user: {Email}", user.Email);

        try {
            var analytics = await _analyticsService.GetAppAnalyticsAsync(user.Id);
            return Ok(analytics);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error getting app analytics");
            return StatusCode(500, new { error = "Failed to get analytics" });
        }
    }
}

public record CreateSmartPlaylistWithConversationRequest(
    int ConversationId,
    string Prompt,
    PlaylistPreferences? Preferences = null
);

public record DiscoverNewMusicWithConversationRequest(
    int ConversationId,
    int Limit = 10,
    string[]? SourcePlaylistIds = null
);

public record ScanDuplicatesWithConversationRequest(
    int ConversationId,
    string PlaylistId
);

public record ConfirmRemoveDuplicatesWithConversationRequest(
    int ConversationId,
    string PlaylistId,
    string[] TrackUrisToRemove
);

public record SuggestMusicWithConversationRequest(
    int ConversationId,
    string PlaylistId,
    string Context,
    int Limit = 10
);