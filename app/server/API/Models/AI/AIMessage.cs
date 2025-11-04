namespace API.Models.AI;

public record AIMessage(
    string Role,
    string? Content = null,
    List<AIToolCall>? ToolCalls = null,
    string? ToolCallId = null
);
