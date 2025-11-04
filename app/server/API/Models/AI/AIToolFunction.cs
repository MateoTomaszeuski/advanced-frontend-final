namespace API.Models.AI;

public record AIToolFunction(
    string Name,
    string Description,
    object Parameters
);
