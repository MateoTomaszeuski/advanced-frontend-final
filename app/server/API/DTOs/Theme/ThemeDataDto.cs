namespace API.DTOs.Theme;

public class ThemeDataDto {
    public required string PrimaryColor { get; set; }
    public required string SecondaryColor { get; set; }
    public required string AccentColor { get; set; }
    public required string BackgroundColor { get; set; }
    public required string TextColor { get; set; }
    public required string SidebarColor { get; set; }
    public required string CardBackground { get; set; }
    public required string BorderColor { get; set; }
}