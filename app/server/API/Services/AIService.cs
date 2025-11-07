using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using API.Models.AI;

namespace API.Services;

public interface IAIService
{
    Task<AIResponse> GetChatCompletionAsync(List<AIMessage> messages, List<AITool>? tools = null);
    Task<string> GenerateSummaryAsync(List<AIMessage> messages);
    int EstimateTokenCount(string text);
    int CountMessageTokens(List<AIMessage> messages);
    bool NeedsSummarization(int tokenCount);
}

public class AIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AIService> _logger;
    private readonly string _apiBaseUrl;
    private readonly string _apiKey;
    private readonly string _model;
    private const int SummaryTriggerTokens = 100000;

    public AIService(HttpClient httpClient, IConfiguration configuration, ILogger<AIService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        _apiBaseUrl = configuration["AI:ApiBaseUrl"] ?? "https://ai-snow.reindeer-pinecone.ts.net/api/chat/completions";
        _model = configuration["AI:Model"] ?? "gpt-oss-120b";
        _apiKey = configuration["AI:ApiKey"] ?? throw new InvalidOperationException("AI:ApiKey is required in configuration");
    }

    public int EstimateTokenCount(string text)
    {
        return (int)Math.Ceiling(text.Length / 4.0);
    }

    public int CountMessageTokens(List<AIMessage> messages)
    {
        var totalTokens = 0;
        foreach (var msg in messages)
        {
            if (!string.IsNullOrEmpty(msg.Content))
            {
                totalTokens += EstimateTokenCount(msg.Content);
            }
            if (msg.ToolCalls != null)
            {
                totalTokens += EstimateTokenCount(JsonSerializer.Serialize(msg.ToolCalls));
            }
        }
        return totalTokens;
    }

    public bool NeedsSummarization(int tokenCount)
    {
        return tokenCount >= SummaryTriggerTokens;
    }

    public async Task<string> GenerateSummaryAsync(List<AIMessage> messages)
    {
        try
        {
            var conversationText = string.Join("\n\n", messages.Select(m => $"{m.Role}: {m.Content}"));
            
            var summaryMessages = new List<AIMessage>
            {
                new AIMessage(
                    "system",
                    "You are a conversation summarizer. Create a concise but comprehensive summary of the following conversation that preserves all important context, decisions, and information. The summary will be used to maintain context in an ongoing conversation."
                ),
                new AIMessage(
                    "user",
                    $"Please summarize the following conversation:\n\n{conversationText}"
                )
            };

            var response = await GetChatCompletionAsync(summaryMessages);
            return response.Response ?? "Summary generation failed";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating summary");
            return "Unable to generate summary";
        }
    }

    public async Task<AIResponse> GetChatCompletionAsync(List<AIMessage> messages, List<AITool>? tools = null)
    {
        try
        {
            var requestBody = new Dictionary<string, object>
            {
                ["model"] = _model,
                ["messages"] = messages.Select(m =>
                {
                    var msg = new Dictionary<string, object?>
                    {
                        ["role"] = m.Role,
                        ["content"] = m.Content
                    };

                    if (m.ToolCalls != null)
                    {
                        msg["tool_calls"] = m.ToolCalls;
                    }

                    if (!string.IsNullOrEmpty(m.ToolCallId))
                    {
                        msg["tool_call_id"] = m.ToolCallId;
                    }

                    return msg;
                }).ToList(),
                ["temperature"] = 0.9,
                ["top_p"] = 0.95
            };

            if (tools != null && tools.Count > 0)
            {
                requestBody["tools"] = tools;
            }

            var jsonContent = JsonSerializer.Serialize(requestBody);
            _logger.LogDebug("AI API Request: {Request}", jsonContent);

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.PostAsync(_apiBaseUrl, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("AI API request failed: {StatusCode} {Error}", response.StatusCode, errorText);
                return new AIResponse("", Error: $"AI API request failed: {response.StatusCode}");
            }

            var responseText = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("AI API Response: {Response}", responseText);

            var responseData = JsonSerializer.Deserialize<JsonElement>(responseText);
            
            if (!responseData.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
            {
                return new AIResponse("", Error: "No response from AI API");
            }

            var choice = choices[0];
            var message = choice.GetProperty("message");

            // Handle tool calls
            if (message.TryGetProperty("tool_calls", out var toolCallsJson) && toolCallsJson.GetArrayLength() > 0)
            {
                var executedToolCalls = new List<ExecutedToolCall>();

            foreach (var tc in toolCallsJson.EnumerateArray())
            {
                var id = tc.GetProperty("id").GetString()!;
                var functionData = tc.GetProperty("function");
                var name = functionData.GetProperty("name").GetString()!;
                var argumentsJson = functionData.GetProperty("arguments").GetString()!;
                
                var arguments = JsonSerializer.Deserialize<Dictionary<string, object>>(argumentsJson) 
                    ?? new Dictionary<string, object>();

                executedToolCalls.Add(new ExecutedToolCall(id, name, arguments));
            }

            var messageContent = message.TryGetProperty("content", out var contentProp) && contentProp.ValueKind != JsonValueKind.Null
                ? contentProp.GetString()
                : null;

            return new AIResponse(messageContent ?? "", executedToolCalls);
        }

        // Normal text response
        if (!message.TryGetProperty("content", out var contentProperty) || contentProperty.ValueKind == JsonValueKind.Null)
        {
            return new AIResponse("", Error: "No response content from AI API");
        }

        return new AIResponse(contentProperty.GetString() ?? "");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "AI Service error");
        return new AIResponse("", Error: ex.Message);
    }
}
}