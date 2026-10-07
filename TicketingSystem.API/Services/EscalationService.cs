using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.API.Services;

public class EscalationService : IEscalationService
{
    // Simple in-memory tracking - production should use DB
    private static readonly Dictionary<int, DateTime> _activeTimers = new();

    public Task StartEscalationTimer(int ticketId)
    {
        _activeTimers[ticketId] = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    public Task StopEscalationTimer(int ticketId)
    {
        _activeTimers.Remove(ticketId);
        return Task.CompletedTask;
    }
}
