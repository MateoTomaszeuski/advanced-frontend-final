using API.DTOs.Theme;

namespace API.Interfaces;

public interface IThemeService {
    Task<ThemeDataDto> GenerateThemeAsync(string description);
    Task<ThemeResponse> SaveThemeAsync(int userId, ThemeDataDto themeData, string description);
    Task<ThemeResponse?> GetUserThemeAsync(int userId);
    Task DeleteThemeAsync(int userId);
}