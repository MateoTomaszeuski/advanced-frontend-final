using API.DTOs.Theme;
using API.Models;
using API.Models.AI;
using API.Repositories;
using System.Text.Json;

namespace API.Services;

public interface IThemeService
{
    Task<ThemeDataDto> GenerateThemeAsync(string description);
    Task<ThemeResponse> SaveThemeAsync(int userId, ThemeDataDto themeData, string description);
    Task<ThemeResponse?> GetUserThemeAsync(int userId);
    Task DeleteThemeAsync(int userId);
}

public class ThemeService : IThemeService
{
    private readonly IThemeRepository _themeRepository;
    private readonly IAIService _aiService;
    private readonly ILogger<ThemeService> _logger;

    public ThemeService(
        IThemeRepository themeRepository,
        IAIService aiService,
        ILogger<ThemeService> logger)
    {
        _themeRepository = themeRepository;
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<ThemeDataDto> GenerateThemeAsync(string description)
    {
        var tools = new List<AITool>
        {
            new AITool(
                "function",
                new AIToolFunction(
                    "setAppTheme",
                    "Set the application's visual theme colors based on user preferences. Use this to customize the look and feel of the app.",
                    new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object>
                        {
                            ["primaryColor"] = new Dictionary<string, string>
                            {
                                ["type"] = "string",
                                ["description"] = "Primary color in hex format (e.g., #3B82F6)"
                            },
                            ["secondaryColor"] = new Dictionary<string, string>
                            {
                                ["type"] = "string",
                                ["description"] = "Secondary color in hex format"
                            },
                            ["accentColor"] = new Dictionary<string, string>
                            {
                                ["type"] = "string",
                                ["description"] = "Accent color for highlights and CTAs in hex format"
                            },
                            ["backgroundColor"] = new Dictionary<string, string>
                            {
                                ["type"] = "string",
                                ["description"] = "Main background color in hex format"
                            },
                            ["textColor"] = new Dictionary<string, string>
                            {
                                ["type"] = "string",
                                ["description"] = "Primary text color in hex format"
                            },
                            ["sidebarColor"] = new Dictionary<string, string>
                            {
                                ["type"] = "string",
                                ["description"] = "Sidebar background color in hex format"
                            },
                            ["cardBackground"] = new Dictionary<string, string>
                            {
                                ["type"] = "string",
                                ["description"] = "Card/panel background color in hex format"
                            },
                            ["borderColor"] = new Dictionary<string, string>
                            {
                                ["type"] = "string",
                                ["description"] = "Border and divider color in hex format"
                            }
                        },
                        ["required"] = new[] { "primaryColor", "secondaryColor", "accentColor", "backgroundColor", "textColor", "sidebarColor", "cardBackground", "borderColor" }
                    }
                )
            )
        };

        var messages = new List<AIMessage>
        {
            new AIMessage("system", "You are a professional UI/UX designer and color specialist. Based on user descriptions, you generate harmonious, accessible color themes for web applications. Always use the setAppTheme function to return your theme design. Ensure colors have good contrast ratios for accessibility."),
            new AIMessage("user", $"Create a beautiful, cohesive color theme for a Spotify music management app based on this description: {description}")
        };

        var response = await _aiService.GetChatCompletionAsync(messages, tools);

        // Check if AI service returned an error
        if (!string.IsNullOrEmpty(response.Error))
        {
            throw new InvalidOperationException($"Failed to generate theme: {response.Error}");
        }

        if (response.ToolCalls == null || !response.ToolCalls.Any())
        {
            throw new InvalidOperationException("AI did not generate a theme. Please try a different description.");
        }

        var toolCall = response.ToolCalls.First(tc => tc.Name == "setAppTheme");
        var themeArgs = toolCall.Arguments;

        return new ThemeDataDto
        {
            PrimaryColor = themeArgs["primaryColor"]?.ToString() ?? "#3B82F6",
            SecondaryColor = themeArgs["secondaryColor"]?.ToString() ?? "#8B5CF6",
            AccentColor = themeArgs["accentColor"]?.ToString() ?? "#10B981",
            BackgroundColor = themeArgs["backgroundColor"]?.ToString() ?? "#FFFFFF",
            TextColor = themeArgs["textColor"]?.ToString() ?? "#111827",
            SidebarColor = themeArgs["sidebarColor"]?.ToString() ?? "#F9FAFB",
            CardBackground = themeArgs["cardBackground"]?.ToString() ?? "#FFFFFF",
            BorderColor = themeArgs["borderColor"]?.ToString() ?? "#E5E7EB"
        };
    }

    public async Task<ThemeResponse> SaveThemeAsync(int userId, ThemeDataDto themeData, string description)
    {
        var existingTheme = await _themeRepository.GetByUserIdAsync(userId);
        var now = DateTime.UtcNow;

        var themeDataJson = JsonSerializer.Serialize(themeData);

        if (existingTheme != null)
        {
            existingTheme.ThemeData = themeDataJson;
            existingTheme.Description = description;
            existingTheme.UpdatedAt = now;
            await _themeRepository.UpdateAsync(existingTheme);

            return new ThemeResponse
            {
                ThemeData = themeData,
                Description = description,
                CreatedAt = existingTheme.CreatedAt,
                UpdatedAt = now
            };
        }
        else
        {
            var newTheme = new UserTheme
            {
                UserId = userId,
                ThemeData = themeDataJson,
                Description = description,
                CreatedAt = now,
                UpdatedAt = now
            };

            var created = await _themeRepository.CreateAsync(newTheme);

            return new ThemeResponse
            {
                ThemeData = themeData,
                Description = description,
                CreatedAt = created.CreatedAt,
                UpdatedAt = created.UpdatedAt
            };
        }
    }

    public async Task<ThemeResponse?> GetUserThemeAsync(int userId)
    {
        var theme = await _themeRepository.GetByUserIdAsync(userId);
        
        if (theme == null)
            return null;

        var themeData = JsonSerializer.Deserialize<ThemeDataDto>(theme.ThemeData);
        
        if (themeData == null)
            return null;

        return new ThemeResponse
        {
            ThemeData = themeData,
            Description = theme.Description,
            CreatedAt = theme.CreatedAt,
            UpdatedAt = theme.UpdatedAt
        };
    }

    public async Task DeleteThemeAsync(int userId)
    {
        await _themeRepository.DeleteAsync(userId);
    }
}
