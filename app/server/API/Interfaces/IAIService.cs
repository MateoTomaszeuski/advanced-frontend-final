using API.Models.AI;

namespace API.Interfaces;

public interface IAIService {
    Task<AIResponse> GetChatCompletionAsync(List<AIMessage> messages, List<AITool>? tools = null);
    Task<string> GenerateSummaryAsync(List<AIMessage> messages);
    int EstimateTokenCount(string text);
    int CountMessageTokens(List<AIMessage> messages);
    bool NeedsSummarization(int tokenCount);
}