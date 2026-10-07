namespace TicketingSystem.Core.Interfaces;

public interface INotificationService
{
    Task NotifyAsync(int userId, string message, string type, int? ticketId = null);
    Task NotifyGroupAsync(string group, string message, string type, int? ticketId = null);
}
