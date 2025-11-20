namespace API.Models.AI;

public record AITool(
    string Type,
    AIToolFunction Function
);