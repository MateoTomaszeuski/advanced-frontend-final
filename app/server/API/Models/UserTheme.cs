namespace API.Models;

public class UserTheme {
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string ThemeData { get; set; }
    public required string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}