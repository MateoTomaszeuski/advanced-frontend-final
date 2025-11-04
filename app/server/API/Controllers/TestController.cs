using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    [HttpGet("me")]
    public IActionResult GetMe()
    {
        var email = this.GetUserEmail();
        var user = this.GetCurrentUser();

        if (email == null || user == null)
        {
            return this.UnauthorizedUser();
        }

        _logger.LogInformation("Test endpoint accessed by user: {Email}", email);

        return Ok(new
        {
            email = email,
            userId = user.Id,
            displayName = user.DisplayName,
            message = "Authentication successful!"
        });
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        var email = this.GetUserEmail();
        _logger.LogInformation("Ping endpoint accessed by user: {Email}", email ?? "anonymous");

        return Ok(new
        {
            message = "pong",
            timestamp = DateTime.UtcNow,
            userEmail = email
        });
    }
}
