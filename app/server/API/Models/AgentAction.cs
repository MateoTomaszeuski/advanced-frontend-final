using System.Text.Json;

namespace API.Models;

public class AgentAction {
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public required string ActionType { get; set; }
    public required string Status { get; set; }
    public string? InputPrompt { get; set; }
    public JsonDocument? Parameters { get; set; }
    public JsonDocument? Result { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}