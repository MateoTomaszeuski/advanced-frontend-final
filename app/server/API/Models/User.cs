namespace API.Models;

public class User {
    public int Id { get; set; }
    public required string Email { get; set; }
    public string? DisplayName { get; set; }
    public string? SpotifyAccessToken { get; set; }
    public string? SpotifyRefreshToken { get; set; }
    public DateTime? SpotifyTokenExpiry { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}