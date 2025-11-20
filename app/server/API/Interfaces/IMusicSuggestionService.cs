using API.DTOs.Agent;
using API.Models;

namespace API.Interfaces;

public interface IMusicSuggestionService {
    Task<SuggestMusicResponse> SuggestMusicByContextAsync(
        User user,
        SuggestMusicRequest request,
        int conversationId);
}