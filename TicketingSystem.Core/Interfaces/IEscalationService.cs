namespace TicketingSystem.Core.Interfaces;

public interface IEscalationService
{
    Task StartEscalationTimer(int ticketId);
    Task StopEscalationTimer(int ticketId);
}
