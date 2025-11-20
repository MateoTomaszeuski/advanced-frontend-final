namespace API.DTOs.Theme;

public class ThemeResponse {
    public required ThemeDataDto ThemeData { get; set; }
    public required string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}