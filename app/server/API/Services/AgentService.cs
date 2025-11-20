using API.DTOs.Agent;
using API.Interfaces;
using API.Models;

namespace API.Services;

public class AgentService : IAgentService {
    private readonly IDuplicateCleanerService _duplicateCleanerService;
    private readonly IPlaylistCreatorService _playlistCreatorService;
    private readonly IMusicDiscoveryService _musicDiscoveryService;
    private readonly IMusicSuggestionService _musicSuggestionService;

    public AgentService(
        IDuplicateCleanerService duplicateCleanerService,
        IPlaylistCreatorService playlistCreatorService,
        IMusicDiscoveryService musicDiscoveryService,
        IMusicSuggestionService musicSuggestionService) {
        _duplicateCleanerService = duplicateCleanerService;
        _playlistCreatorService = playlistCreatorService;
        _musicDiscoveryService = musicDiscoveryService;
        _musicSuggestionService = musicSuggestionService;
    }

    public async Task<RemoveDuplicatesResponse> ScanForDuplicatesAsync(User user, string playlistId, int conversationId)
        => await _duplicateCleanerService.ScanForDuplicatesAsync(user, playlistId, conversationId);

    public async Task<AgentActionResponse> ConfirmRemoveDuplicatesAsync(User user, ConfirmRemoveDuplicatesRequest request, int conversationId)
        => await _duplicateCleanerService.ConfirmRemoveDuplicatesAsync(user, request, conversationId);

    public async Task<AgentActionResponse> CreateSmartPlaylistAsync(User user, CreateSmartPlaylistRequest request, int conversationId)
        => await _playlistCreatorService.CreateSmartPlaylistAsync(user, request, conversationId);

    public async Task<AgentActionResponse> DiscoverNewMusicAsync(User user, DiscoverNewMusicRequest request, int conversationId)
        => await _musicDiscoveryService.DiscoverNewMusicAsync(user, request, conversationId);

    public async Task<SuggestMusicResponse> SuggestMusicByContextAsync(User user, SuggestMusicRequest request, int conversationId)
        => await _musicSuggestionService.SuggestMusicByContextAsync(user, request, conversationId);
}