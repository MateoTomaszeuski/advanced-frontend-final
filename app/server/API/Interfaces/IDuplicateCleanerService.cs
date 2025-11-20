using API.DTOs.Agent;
using API.Models;

namespace API.Interfaces;

public interface IDuplicateCleanerService {
    Task<RemoveDuplicatesResponse> ScanForDuplicatesAsync(
        User user,
        string playlistId,
        int conversationId);

    Task<AgentActionResponse> ConfirmRemoveDuplicatesAsync(
        User user,
        ConfirmRemoveDuplicatesRequest request,
        int conversationId);
}