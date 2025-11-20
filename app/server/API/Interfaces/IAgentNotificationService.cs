namespace API.Interfaces;

public interface IAgentNotificationService {
    Task SendStatusUpdateAsync(string userEmail, string status, string? message = null, object? data = null);
}