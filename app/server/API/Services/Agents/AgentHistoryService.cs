using API.Interfaces;
using API.Repositories;

namespace API.Services.Agents;

public class AgentHistoryService : IAgentHistoryService {
    private readonly IAgentActionRepository _actionRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly ILogger<AgentHistoryService> _logger;

    public AgentHistoryService(
        IAgentActionRepository actionRepository,
        IConversationRepository conversationRepository,
        ILogger<AgentHistoryService> logger) {
        _actionRepository = actionRepository;
        _conversationRepository = conversationRepository;
        _logger = logger;
    }

    public async Task<(object? action, bool isOwner)> GetActionByIdAsync(int actionId, int userId) {
        var action = await _actionRepository.GetByIdAsync(actionId);
        if (action == null) {
            return (null, false);
        }

        var conversation = await _conversationRepository.GetByIdAsync(action.ConversationId);
        if (conversation == null || conversation.UserId != userId) {
            return (null, false);
        }

        var result = new {
            action.Id,
            action.ActionType,
            action.Status,
            action.InputPrompt,
            action.Parameters,
            action.Result,
            action.ErrorMessage,
            action.CreatedAt,
            action.CompletedAt
        };

        return (result, true);
    }

    public async Task<IEnumerable<object>> GetHistoryAsync(int userId, string? actionType, string? status, int limit) {
        var allConversations = await _conversationRepository.GetAllByUserIdAsync(userId);
        var conversationIds = allConversations.Select(c => c.Id).ToHashSet();

        var allActions = new List<Models.AgentAction>();
        foreach (var convId in conversationIds) {
            var actions = await _actionRepository.GetAllByConversationIdAsync(convId);
            allActions.AddRange(actions);
        }

        var filteredActions = allActions.AsEnumerable();

        if (!string.IsNullOrEmpty(actionType)) {
            filteredActions = filteredActions.Where(a =>
                a.ActionType.Equals(actionType, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(status)) {
            filteredActions = filteredActions.Where(a =>
                a.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        return filteredActions
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .Select(a => new {
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
    }

    public async Task<IEnumerable<object>> GetRecentPlaylistsAsync(int userId, int limit) {
        var actions = await _actionRepository.GetRecentPlaylistCreationsAsync(userId, limit);

        return actions.Select(a => new {
            a.Id,
            a.ActionType,
            a.InputPrompt,
            Result = a.Result,
            a.CreatedAt
        });
    }

    public async Task<int> ClearHistoryAsync(int userId) {
        var allConversations = await _conversationRepository.GetAllByUserIdAsync(userId);
        var conversationIds = allConversations.Select(c => c.Id).ToList();

        var deletedCount = 0;
        foreach (var convId in conversationIds) {
            var actions = await _actionRepository.GetAllByConversationIdAsync(convId);
            foreach (var action in actions) {
                await _actionRepository.DeleteAsync(action.Id);
                deletedCount++;
            }
        }

        return deletedCount;
    }
}