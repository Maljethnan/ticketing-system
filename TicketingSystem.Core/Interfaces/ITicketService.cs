using System.Collections.Generic;
using System.Threading.Tasks;
using TicketingSystem.Core.DTOs;

namespace TicketingSystem.Core.Interfaces;

public interface ITicketService
{
    Task<int> CreateTicketAsync(CreateTicketDto dto, int userId);
    Task<TicketDto?> GetTicketAsync(int ticketId, int userId);
    Task<List<TicketDto>> GetTicketsAsync(FilterRequest filter, int userId);
    Task<List<TicketDto>> GetMyTicketsAsync(int userId, FilterRequest filter);
    Task UpdateTicketStatusAsync(int ticketId, StatusUpdateRequest request, int userId);
    Task CloseTicketAsync(int ticketId, CloseRequest request, int userId);
    Task ApproveClosureAsync(int ticketId, int userId);
    Task ReopenTicketAsync(int ticketId, ReopenRequest request, int userId);
    Task GrantAccessAsync(int ticketId, GrantAccessRequest request, int userId);
}
