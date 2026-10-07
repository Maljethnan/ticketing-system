using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Data.Context;

namespace TicketingSystem.API.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _context;

    public AuditService(AppDbContext context) => _context = context;

    public async Task LogAsync(int userId, string action, string entity, int entityId, string details)
    {
        var log = new AuditLog
        {
            UserId = userId, Action = action, Entity = entity,
            EntityId = entityId, Details = details, LoggedAt = DateTime.UtcNow
        };
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
