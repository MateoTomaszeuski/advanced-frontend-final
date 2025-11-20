using API.DTOs.Agent;
using API.Models;

namespace API.Interfaces;

public interface IAgentService {
    Task<AgentActionResponse> CreateSmartPlaylistAsync(User user, CreateSmartPlaylistRequest request, int conversationId);
    Task<AgentActionResponse> DiscoverNewMusicAsync(User user, DiscoverNewMusicRequest request, int conversationId);
    Task<RemoveDuplicatesResponse> ScanForDuplicatesAsync(User user, string playlistId, int conversationId);
    Task<AgentActionResponse> ConfirmRemoveDuplicatesAsync(User user, ConfirmRemoveDuplicatesRequest request, int conversationId);
    Task<SuggestMusicResponse> SuggestMusicByContextAsync(User user, SuggestMusicRequest request, int conversationId);
}