using API.DTOs.Theme;
using API.Extensions;
using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ThemesController : ControllerBase {
    private readonly IThemeService _themeService;
    private readonly ILogger<ThemesController> _logger;

    public ThemesController(
        IThemeService themeService,
        ILogger<ThemesController> logger) {
        _themeService = themeService;
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<ThemeDataDto>> GenerateTheme([FromBody] GenerateThemeRequest request) {
        try {
            var theme = await _themeService.GenerateThemeAsync(request.Description);
            return Ok(theme);
        } catch (InvalidOperationException ex) {
            return BadRequest(new { error = ex.Message });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error generating theme");
            return StatusCode(500, new { error = "Failed to generate theme" });
        }
    }

    [HttpPost("save")]
    public async Task<ActionResult<ThemeResponse>> SaveTheme([FromBody] SaveThemeRequest request) {
        try {
            var user = this.GetCurrentUser();
            if (user == null) {
                return Unauthorized(new { error = "User not authenticated" });
            }

            var theme = await _themeService.SaveThemeAsync(user.Id, request.ThemeData, request.Description);

            return Ok(theme);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error saving theme");
            return StatusCode(500, new { error = "Failed to save theme" });
        }
    }

    [HttpGet("current")]
    public async Task<ActionResult<ThemeResponse>> GetCurrentTheme() {
        try {
            var user = this.GetCurrentUser();
            if (user == null) {
                return Unauthorized(new { error = "User not authenticated" });
            }

            var theme = await _themeService.GetUserThemeAsync(user.Id);

            if (theme == null) {
                return NotFound(new { error = "No theme found" });
            }

            return Ok(theme);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error getting current theme");
            return StatusCode(500, new { error = "Failed to get theme" });
        }
    }

    [HttpDelete("current")]
    public async Task<ActionResult> DeleteTheme() {
        try {
            var user = this.GetCurrentUser();
            if (user == null) {
                return Unauthorized(new { error = "User not authenticated" });
            }

            await _themeService.DeleteThemeAsync(user.Id);

            return Ok(new { message = "Theme deleted successfully" });
        } catch (Exception ex) {
            _logger.LogError(ex, "Error deleting theme");
            return StatusCode(500, new { error = "Failed to delete theme" });
        }
    }
}