namespace API.DTOs.Spotify;

public record SpotifyUserProfile(
    string Id,
    string DisplayName,
    string Email,
    string? Country,
    string? ImageUrl
);