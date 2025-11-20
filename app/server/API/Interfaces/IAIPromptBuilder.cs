using API.Models.AI;

namespace API.Interfaces;

public interface IAIPromptBuilder {
    List<AIMessage> BuildPlaylistCreationPrompt(string userPrompt);
    List<AIMessage> BuildDiscoveryPrompt(string[] topTrackDescriptions);
    List<AIMessage> BuildGenrePrompt();
    List<AIMessage> BuildAdaptiveSearchPrompt(string originalPrompt, string[] currentQueries, int found, int needed);
    List<AIMessage> BuildSuggestionPrompt(string playlistName, string[] topTrackDescriptions, string context);
}