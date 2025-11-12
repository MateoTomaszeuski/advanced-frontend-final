using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;

[Authorize]
public class AgentHub : Hub
{
    private readonly ILogger<AgentHub> _logger;

    public AgentHub(ILogger<AgentHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userEmail = Context.User?.FindFirst("email")?.Value ?? "anonymous";
        var userName = Context.User?.FindFirst("name")?.Value ?? "Unknown";
        var connectionId = Context.ConnectionId;
        var timestamp = DateTime.UtcNow;
        var transport = Context.Features.Get<Microsoft.AspNetCore.Http.Connections.Features.IHttpTransportFeature>()?.TransportType.ToString() ?? "Unknown";
        
        _logger.LogInformation(
            "WebSocket CONNECTED - ConnectionId: {ConnectionId}, User: {Email} ({Name}), Time: {Timestamp}, Transport: {Transport}",
            connectionId,
            userEmail,
            userName,
            timestamp,
            transport
        );
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userEmail = Context.User?.FindFirst("email")?.Value ?? "anonymous";
        var userName = Context.User?.FindFirst("name")?.Value ?? "Unknown";
        var connectionId = Context.ConnectionId;
        var timestamp = DateTime.UtcNow;
        
        if (exception != null)
        {
            _logger.LogWarning(
                exception,
                "WebSocket DISCONNECTED WITH ERROR - ConnectionId: {ConnectionId}, User: {Email} ({Name}), Time: {Timestamp}",
                connectionId,
                userEmail,
                userName,
                timestamp
            );
        }
        else
        {
            _logger.LogInformation(
                "WebSocket DISCONNECTED - ConnectionId: {ConnectionId}, User: {Email} ({Name}), Time: {Timestamp}",
                connectionId,
                userEmail,
                userName,
                timestamp
            );
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinUserGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        _logger.LogInformation("Client {ConnectionId} joined group user-{UserId}", Context.ConnectionId, userId);
    }

    public async Task LeaveUserGroup(string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
        _logger.LogInformation("Client {ConnectionId} left group user-{UserId}", Context.ConnectionId, userId);
    }
}
