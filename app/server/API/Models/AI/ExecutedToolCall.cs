namespace API.Models.AI;

public record ExecutedToolCall(
    string Id,
    string Name,
    Dictionary<string, object> Arguments
);
