using API.DTOs.Agent;
using API.Models;

namespace API.Interfaces;

public interface IPlaylistCreatorService {
    Task<AgentActionResponse> CreateSmartPlaylistAsync(
        User user,
        CreateSmartPlaylistRequest request,
        int conversationId);
}