namespace API.DTOs.Analytics;

public record AppAnalyticsResponse(
    UserActivityStats UserActivity,
    ActionTypeStats ActionTypes,
    Dictionary<string, int> ActionsOverTime,
    Dictionary<string, int> PlaylistsByGenre,
    DuplicateStats Duplicates
);

public record UserActivityStats(
    int TotalActions,
    int CompletedActions,
    int FailedActions,
    int TotalConversations,
    int TotalPlaylistsCreated,
    int TotalTracksDiscovered
);

public record ActionTypeStats(
    int SmartPlaylists,
    int MusicDiscovery,
    int DuplicateScans,
    int DuplicateRemovals,
    int MusicSuggestions
);

public record DuplicateStats(
    int TotalScans,
    int TotalDuplicatesFound,
    int TotalDuplicatesRemoved,
    double AverageDuplicatesPerPlaylist
);