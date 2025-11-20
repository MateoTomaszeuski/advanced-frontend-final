namespace API.Models.AI;

public record AIToolCall(
    string Id,
    string Type,
    AIFunction Function
);