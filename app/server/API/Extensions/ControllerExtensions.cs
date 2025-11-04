using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions;

public static class ControllerExtensions
{
    public static string? GetUserEmail(this ControllerBase controller)
    {
        return controller.HttpContext.Items["UserEmail"] as string;
    }

    public static User? GetCurrentUser(this ControllerBase controller)
    {
        return controller.HttpContext.Items["User"] as User;
    }

    public static IActionResult UnauthorizedUser(this ControllerBase controller)
    {
        return controller.Unauthorized(new { error = "User not authenticated" });
    }
}
