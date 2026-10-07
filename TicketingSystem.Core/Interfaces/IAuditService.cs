namespace TicketingSystem.Core.Interfaces;

public interface IAuditService
{
    Task LogAsync(int userId, string action, string entity, int entityId, string details);
}
