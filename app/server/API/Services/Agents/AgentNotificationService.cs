using API.Hubs;
using API.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace API.Services.Agents;

public class AgentNotificationService : IAgentNotificationService {
    private readonly IHubContext<AgentHub> _hubContext;
    private readonly ILogger<AgentNotificationService> _logger;

    public AgentNotificationService(
        IHubContext<AgentHub> hubContext,
        ILogger<AgentNotificationService> logger) {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendStatusUpdateAsync(string userEmail, string status, string? message = null, object? data = null) {
        try {
            await _hubContext.Clients.Group($"user-{userEmail}").SendAsync("AgentStatusUpdate", new {
                status,
                message,
                data,
                timestamp = DateTime.UtcNow
            });
        } catch (Exception ex) {
            _logger.LogWarning(ex, "Failed to send WebSocket status update to user {Email}", userEmail);
        }
    }
}