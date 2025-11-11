using API.DTOs.Agent;
using API.Extensions;
using API.Repositories;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgentController : ControllerBase
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IAgentActionRepository _actionRepository;
    private readonly IAgentService _agentService;
    private readonly ILogger<AgentController> _logger;

    public AgentController(
        IConversationRepository conversationRepository,
        IAgentActionRepository actionRepository,
        IAgentService agentService,
        ILogger<AgentController> logger)
    {
        _conversationRepository = conversationRepository;
        _actionRepository = actionRepository;
        _agentService = agentService;
        _logger = logger;
    }

    [HttpPost("create-smart-playlist")]
    public async Task<IActionResult> CreateSmartPlaylist([FromBody] CreateSmartPlaylistWithConversationRequest request)
    {
        var user = this.GetCurrentUser();
        if (user == null)
        {
            return this.UnauthorizedUser();
        }

        _logger.LogInformation("CreateSmartPlaylist request from user: {Email}", user.Email);

        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId);
        if (conversation == null || conversation.UserId != user.Id)
        {
            return BadRequest(new { error = "Invalid conversation" });
        }

        var result = await _agentService.CreateSmartPlaylistAsync(
            user,
            new CreateSmartPlaylistRequest(request.Prompt, request.Preferences),
            request.ConversationId
        );

        if (result.Status == "Failed")
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("discover-new-music")]
    public async Task<IActionResult> DiscoverNewMusic([FromBody] DiscoverNewMusicWithConversationRequest request)
    {
        var user = this.GetCurrentUser();
        if (user == null)
        {
            return this.UnauthorizedUser();
        }

        _logger.LogInformation("DiscoverNewMusic request from user: {Email}", user.Email);

        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId);
        if (conversation == null || conversation.UserId != user.Id)
        {
            return BadRequest(new { error = "Invalid conversation" });
        }

        var result = await _agentService.DiscoverNewMusicAsync(
            user,
            new DiscoverNewMusicRequest(request.Limit, request.SourcePlaylistIds),
            request.ConversationId
        );

        if (result.Status == "Failed")
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("actions/{actionId}")]
    public async Task<IActionResult> GetAction(int actionId)
    {
        var user = this.GetCurrentUser();
        if (user == null)
        {
            return this.UnauthorizedUser();
        }

        _logger.LogInformation("GetAction request for action {ActionId} from user: {Email}", actionId, user.Email);

        var action = await _actionRepository.GetByIdAsync(actionId);

        if (action == null)
        {
            return NotFound(new { error = "Action not found" });
        }

        var conversation = await _conversationRepository.GetByIdAsync(action.ConversationId);
        if (conversation == null || conversation.UserId != user.Id)
        {
            _logger.LogWarning("User {Email} attempted to access action {ActionId} from another user",
                user.Email, actionId);
            return Forbid();
        }

        return Ok(new
        {
            action.Id,
            action.ActionType,
            action.Status,
            action.InputPrompt,
            action.Parameters,
            action.Result,
            action.ErrorMessage,
            action.CreatedAt,
            action.CompletedAt
        });
    }

    [HttpPost("scan-duplicates")]
    public async Task<IActionResult> ScanDuplicates([FromBody] ScanDuplicatesWithConversationRequest request)
    {
        var user = this.GetCurrentUser();
        if (user == null)
        {
            return this.UnauthorizedUser();
        }

        _logger.LogInformation("ScanDuplicates request from user: {Email} for playlist: {PlaylistId}", 
            user.Email, request.PlaylistId);

        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId);
        if (conversation == null || conversation.UserId != user.Id)
        {
            return BadRequest(new { error = "Invalid conversation" });
        }

        try
        {
            var result = await _agentService.ScanForDuplicatesAsync(
                user,
                request.PlaylistId,
                request.ConversationId
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning for duplicates");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("confirm-remove-duplicates")]
    public async Task<IActionResult> ConfirmRemoveDuplicates([FromBody] ConfirmRemoveDuplicatesWithConversationRequest request)
    {
        var user = this.GetCurrentUser();
        if (user == null)
        {
            return this.UnauthorizedUser();
        }

        _logger.LogInformation("ConfirmRemoveDuplicates request from user: {Email}", user.Email);

        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId);
        if (conversation == null || conversation.UserId != user.Id)
        {
            return BadRequest(new { error = "Invalid conversation" });
        }

        var result = await _agentService.ConfirmRemoveDuplicatesAsync(
            user,
            new ConfirmRemoveDuplicatesRequest(request.PlaylistId, request.TrackUrisToRemove),
            request.ConversationId
        );

        if (result.Status == "Failed")
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("suggest-music")]
    public async Task<IActionResult> SuggestMusic([FromBody] SuggestMusicWithConversationRequest request)
    {
        var user = this.GetCurrentUser();
        if (user == null)
        {
            return this.UnauthorizedUser();
        }

        _logger.LogInformation("SuggestMusic request from user: {Email} for playlist: {PlaylistId}", 
            user.Email, request.PlaylistId);

        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId);
        if (conversation == null || conversation.UserId != user.Id)
        {
            return BadRequest(new { error = "Invalid conversation" });
        }

        try
        {
            var result = await _agentService.SuggestMusicByContextAsync(
                user,
                new SuggestMusicRequest(request.PlaylistId, request.Context, request.Limit),
                request.ConversationId
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suggesting music");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("recent-playlists")]
    public async Task<IActionResult> GetRecentlyCreatedPlaylists([FromQuery] int limit = 10)
    {
        var user = this.GetCurrentUser();
        if (user == null)
        {
            return this.UnauthorizedUser();
        }

        _logger.LogInformation("GetRecentlyCreatedPlaylists request from user: {Email}", user.Email);

        try
        {
            var actions = await _actionRepository.GetRecentPlaylistCreationsAsync(user.Id, limit);
            
            return Ok(actions.Select(a => new
            {
                a.Id,
                a.ActionType,
                a.InputPrompt,
                Result = a.Result,
                a.CreatedAt
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent playlists");
            return StatusCode(500, new { error = "Failed to fetch recent playlists" });
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] string? actionType = null, [FromQuery] string? status = null, [FromQuery] int limit = 50)
    {
        var user = this.GetCurrentUser();
        if (user == null)
        {
            return this.UnauthorizedUser();
        }

        _logger.LogInformation("GetHistory request for user: {Email}", user.Email);

        try
        {
            var allConversations = await _conversationRepository.GetAllByUserIdAsync(user.Id);
            var conversationIds = allConversations.Select(c => c.Id).ToHashSet();

            var allActions = new List<Models.AgentAction>();

            foreach (var convId in conversationIds)
            {
                var actions = await _actionRepository.GetAllByConversationIdAsync(convId);
                allActions.AddRange(actions);
            }

            var filteredActions = allActions.AsEnumerable();

            if (!string.IsNullOrEmpty(actionType))
            {
                filteredActions = filteredActions.Where(a => a.ActionType.Equals(actionType, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(status))
            {
                filteredActions = filteredActions.Where(a => a.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            var result = filteredActions
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .Select(a => new
                {
                    a.Id,
                    a.ConversationId,
                    a.ActionType,
                    a.Status,
                    a.InputPrompt,
                    a.Parameters,
                    a.Result,
                    a.ErrorMessage,
                    a.CreatedAt,
                    a.CompletedAt
                });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting action history");
            return StatusCode(500, new { error = "Failed to get recent playlists" });
        }
    }

    [HttpGet("analytics")]
    public async Task<IActionResult> GetAppAnalytics()
    {
        var user = this.GetCurrentUser();
        if (user == null)
        {
            return this.UnauthorizedUser();
        }

        _logger.LogInformation("GetAppAnalytics request from user: {Email}", user.Email);

        try
        {
            // Get action type counts
            var actionTypeCounts = await _actionRepository.GetActionTypeCountsAsync(user.Id);
            
            // Get actions over time (last 30 days)
            var actionsOverTime = await _actionRepository.GetActionsOverTimeAsync(user.Id, 30);
            
            // Get total conversations
            var conversations = await _conversationRepository.GetAllByUserIdAsync(user.Id);
            var totalConversations = conversations.Count();
            
            // Calculate totals
            var totalActions = actionTypeCounts.Values.Sum();
            var smartPlaylists = actionTypeCounts.GetValueOrDefault("CreateSmartPlaylist", 0);
            var musicDiscovery = actionTypeCounts.GetValueOrDefault("DiscoverNewMusic", 0);
            var duplicateScans = actionTypeCounts.GetValueOrDefault("ScanDuplicates", 0);
            var duplicateRemovals = actionTypeCounts.GetValueOrDefault("RemoveDuplicates", 0);
            var musicSuggestions = actionTypeCounts.GetValueOrDefault("SuggestMusicByContext", 0);
            
            // Get duplicate stats
            var totalDuplicatesFound = await _actionRepository.GetTotalDuplicatesFoundAsync(user.Id);
            var totalDuplicatesRemoved = await _actionRepository.GetTotalDuplicatesRemovedAsync(user.Id);
            var avgDuplicates = duplicateScans > 0 ? (double)totalDuplicatesFound / duplicateScans : 0;
            
            var analytics = new API.DTOs.Analytics.AppAnalyticsResponse(
                UserActivity: new API.DTOs.Analytics.UserActivityStats(
                    TotalActions: totalActions,
                    CompletedActions: totalActions,
                    FailedActions: 0,
                    TotalConversations: totalConversations,
                    TotalPlaylistsCreated: smartPlaylists + musicDiscovery,
                    TotalTracksDiscovered: musicDiscovery * 10 // Estimate
                ),
                ActionTypes: new API.DTOs.Analytics.ActionTypeStats(
                    SmartPlaylists: smartPlaylists,
                    MusicDiscovery: musicDiscovery,
                    DuplicateScans: duplicateScans,
                    DuplicateRemovals: duplicateRemovals,
                    MusicSuggestions: musicSuggestions
                ),
                ActionsOverTime: actionsOverTime,
                PlaylistsByGenre: new Dictionary<string, int>(), // TODO: Parse from action results
                Duplicates: new API.DTOs.Analytics.DuplicateStats(
                    TotalScans: duplicateScans,
                    TotalDuplicatesFound: totalDuplicatesFound,
                    TotalDuplicatesRemoved: totalDuplicatesRemoved,
                    AverageDuplicatesPerPlaylist: avgDuplicates
                )
            );

            return Ok(analytics);
        }
        catch (Exception ex)
        {
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
