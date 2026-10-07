using TicketingSystem.Core.DTOs;

namespace TicketingSystem.Core.Interfaces;

public interface ITicketService
{
    Task<TicketDto> CreateTicketAsync(CreateTicketRequest request, int userId);
    Task<TicketDetailDto?> GetTicketAsync(int ticketId, int userId);
    Task<List<TicketListDto>> GetTicketsAsync(FilterRequest filter, int userId);
    Task UpdateTicketStatusAsync(int ticketId, int newStatusId, int userId, string? comment);
    Task CloseTicketAsync(int ticketId, int userId, string resolutionSummary);
    Task ApproveClosureAsync(int ticketId, int userId);
    Task ReopenTicketAsync(int ticketId, int userId, string reason);
    Task GrantAccessAsync(int ticketId, int granteeUserId, int grantedByUserId, string? reason);
    Task<List<TicketListDto>> GetMyTicketsAsync(int userId);
}
