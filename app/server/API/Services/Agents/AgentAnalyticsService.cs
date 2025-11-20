using API.DTOs.Analytics;
using API.Interfaces;
using API.Repositories;

namespace API.Services.Agents;

public class AgentAnalyticsService : IAgentAnalyticsService {
    private readonly IAgentActionRepository _actionRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly ILogger<AgentAnalyticsService> _logger;

    public AgentAnalyticsService(
        IAgentActionRepository actionRepository,
        IConversationRepository conversationRepository,
        ILogger<AgentAnalyticsService> logger) {
        _actionRepository = actionRepository;
        _conversationRepository = conversationRepository;
        _logger = logger;
    }

    public async Task<AppAnalyticsResponse> GetAppAnalyticsAsync(int userId) {
        var actionTypeCounts = await _actionRepository.GetActionTypeCountsAsync(userId);
        var actionsOverTime = await _actionRepository.GetActionsOverTimeAsync(userId, 30);
        var conversations = await _conversationRepository.GetAllByUserIdAsync(userId);
        var totalConversations = conversations.Count();

        var totalActions = actionTypeCounts.Values.Sum();
        var smartPlaylists = actionTypeCounts.GetValueOrDefault("CreateSmartPlaylist", 0);
        var musicDiscovery = actionTypeCounts.GetValueOrDefault("DiscoverNewMusic", 0);
        var duplicateScans = actionTypeCounts.GetValueOrDefault("ScanDuplicates", 0);
        var duplicateRemovals = actionTypeCounts.GetValueOrDefault("RemoveDuplicates", 0);
        var musicSuggestions = actionTypeCounts.GetValueOrDefault("SuggestMusicByContext", 0);

        var totalDuplicatesFound = await _actionRepository.GetTotalDuplicatesFoundAsync(userId);
        var totalDuplicatesRemoved = await _actionRepository.GetTotalDuplicatesRemovedAsync(userId);
        var avgDuplicates = duplicateScans > 0 ? (double)totalDuplicatesFound / duplicateScans : 0;

        return new AppAnalyticsResponse(
            UserActivity: new UserActivityStats(
                TotalActions: totalActions,
                CompletedActions: totalActions,
                FailedActions: 0,
                TotalConversations: totalConversations,
                TotalPlaylistsCreated: smartPlaylists + musicDiscovery,
                TotalTracksDiscovered: musicDiscovery * 10
            ),
            ActionTypes: new ActionTypeStats(
                SmartPlaylists: smartPlaylists,
                MusicDiscovery: musicDiscovery,
                DuplicateScans: duplicateScans,
                DuplicateRemovals: duplicateRemovals,
                MusicSuggestions: musicSuggestions
            ),
            ActionsOverTime: actionsOverTime,
            PlaylistsByGenre: new Dictionary<string, int>(),
            Duplicates: new DuplicateStats(
                TotalScans: duplicateScans,
                TotalDuplicatesFound: totalDuplicatesFound,
                TotalDuplicatesRemoved: totalDuplicatesRemoved,
                AverageDuplicatesPerPlaylist: avgDuplicates
            )
        );
    }
}