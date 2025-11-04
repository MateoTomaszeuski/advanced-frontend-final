namespace API.Models.AI;

public record AIResponse(
    string Response,
    List<ExecutedToolCall>? ToolCalls = null,
    string? Error = null
);
