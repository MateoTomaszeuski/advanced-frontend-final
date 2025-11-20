namespace API.Interfaces;

public interface IAIResponseParser {
    (string playlistName, string searchQuery, string description) ParsePlaylistResponse(string? aiResponse, string fallbackPrompt);
    string[] ParseQueriesResponse(string? aiResponse, string[]? fallback = null);
    string[] ParseGenresResponse(string? aiResponse);
}