namespace API.DTOs.Agent;

public record SuggestMusicRequest(
    string PlaylistId,
    string Context,
    int Limit = 10
);

public record SuggestedTrack(
    string Id,
    string Name,
    string[] Artists,
    string Uri,
    string Reason,
    int Popularity
);

public record SuggestMusicResponse(
    string PlaylistId,
    string PlaylistName,
    string Context,
    int SuggestionCount,
    SuggestedTrack[] Suggestions
);
