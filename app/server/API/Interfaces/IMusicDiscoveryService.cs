using API.DTOs.Agent;
using API.Models;

namespace API.Interfaces;

public interface IMusicDiscoveryService {
    Task<AgentActionResponse> DiscoverNewMusicAsync(
        User user,
        DiscoverNewMusicRequest request,
        int conversationId);
}