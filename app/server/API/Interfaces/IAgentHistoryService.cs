using API.Models;

namespace API.Interfaces;

public interface IAgentHistoryService {
    Task<(object? action, bool isOwner)> GetActionByIdAsync(int actionId, int userId);
    Task<IEnumerable<object>> GetHistoryAsync(int userId, string? actionType, string? status, int limit);
    Task<IEnumerable<object>> GetRecentPlaylistsAsync(int userId, int limit);
    Task<int> ClearHistoryAsync(int userId);
}