using Microsoft.AspNetCore.SignalR;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.API.Hubs;

namespace TicketingSystem.API.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext) => _hubContext = hubContext;

    public async Task NotifyAsync(int userId, string message, string type, int? ticketId = null)
    {
        await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification",
            new { Message = message, Type = type, TicketId = ticketId, Timestamp = DateTime.UtcNow });
    }

    public async Task NotifyGroupAsync(string group, string message, string type, int? ticketId = null)
    {
        await _hubContext.Clients.Group(group).SendAsync("ReceiveNotification",
            new { Message = message, Type = type, TicketId = ticketId, Timestamp = DateTime.UtcNow });
    }
}
