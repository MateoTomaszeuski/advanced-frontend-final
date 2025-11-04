namespace API.DTOs.Agent;

public record CreateSmartPlaylistRequest(
    string Prompt,
    PlaylistPreferences? Preferences = null
);

public record PlaylistPreferences(
    int? MaxTracks = null,
    string[]? Genres = null,
    string? Mood = null,
    int? MinEnergy = null,
    int? MaxEnergy = null,
    int? MinTempo = null,
    int? MaxTempo = null
);

public record DiscoverNewMusicRequest(
    int Limit = 10,
    string[]? SourcePlaylistIds = null
);

public record AgentActionResponse(
    int ActionId,
    string ActionType,
    string Status,
    object? Result = null,
    string? ErrorMessage = null
);
