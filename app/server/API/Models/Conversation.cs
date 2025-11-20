namespace API.Models;

public class Conversation {
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string Title { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int ActionCount { get; set; }
}