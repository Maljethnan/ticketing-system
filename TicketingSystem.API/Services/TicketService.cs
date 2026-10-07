using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketingSystem.Core.DTOs;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Entities;
using TicketingSystem.Data.Context;


namespace TicketingSystem.API.Services;

public class TicketService : ITicketService
{
    private readonly AppDbContext _db;

    public TicketService(AppDbContext db) => _db = db;

    public async Task<int> CreateTicketAsync(CreateTicketDto dto, int userId)
    {
        var priorityId = ResolvePriorityId(dto.Priority);
        var ticket = new Ticket
        {
            Title = dto.Title,
            Description = dto.Description,
            SystemId = dto.SystemId,
            IssueTypeId = dto.CategoryId,
            PriorityId = priorityId,
            CreatorId = userId,
            StatusId = 1 // Open
        };
        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();
        return ticket.TicketId;
    }

    public async Task<TicketDto?> GetTicketAsync(int ticketId, int userId)
    {
        var ticket = await _db.Tickets.FindAsync(ticketId);
        if (ticket == null) return null;
        return MapToDto(ticket);
    }

    public async Task<List<TicketDto>> GetTicketsAsync(FilterRequest filter, int userId)
    {
        var query = _db.Tickets.AsQueryable();
        if (!string.IsNullOrEmpty(filter.Search))
            query = query.Where(t => t.Title.Contains(filter.Search));
        if (filter.StatusId.HasValue)
            query = query.Where(t => t.StatusId == filter.StatusId.Value);
        if (filter.PriorityId.HasValue)
            query = query.Where(t => t.PriorityId == filter.PriorityId.Value);
        if (filter.SystemId.HasValue)
            query = query.Where(t => t.SystemId == filter.SystemId.Value);

        var tickets = await query.ToListAsync();
        return tickets.Select(MapToDto).ToList();
    }

    public async Task<List<TicketDto>> GetMyTicketsAsync(int userId, FilterRequest filter)
    {
        var query = _db.Tickets.Where(t => t.CreatorId == userId);
        if (!string.IsNullOrEmpty(filter.Search))
            query = query.Where(t => t.Title.Contains(filter.Search));
        var tickets = await query.ToListAsync();
        return tickets.Select(MapToDto).ToList();
    }

    public async Task UpdateTicketStatusAsync(int ticketId, StatusUpdateRequest request, int userId)
    {
        var ticket = await _db.Tickets.FindAsync(ticketId);
        if (ticket != null)
        {
            ticket.StatusId = request.StatusId;
            await _db.SaveChangesAsync();
        }
    }

    public async Task CloseTicketAsync(int ticketId, CloseRequest request, int userId)
    {
        var ticket = await _db.Tickets.FindAsync(ticketId);
        if (ticket != null)
        {
            ticket.StatusId = 4; // Closed
            await _db.SaveChangesAsync();
        }
    }

    public async Task ApproveClosureAsync(int ticketId, int userId)
    {
        var ticket = await _db.Tickets.FindAsync(ticketId);
        if (ticket != null && ticket.CreatorId == userId)
        {
            ticket.StatusId = 4; // Closed
            await _db.SaveChangesAsync();
        }
    }

    public async Task ReopenTicketAsync(int ticketId, ReopenRequest request, int userId)
    {
        var ticket = await _db.Tickets.FindAsync(ticketId);
        if (ticket != null)
        {
            ticket.StatusId = 1; // Open
            await _db.SaveChangesAsync();
        }
    }

    public async Task GrantAccessAsync(int ticketId, GrantAccessRequest request, int userId)
    {
        // TODO: Implement access granting logic
    }

    private int ResolvePriorityId(string priorityName)
    {
        var p = _db.Priorities.FirstOrDefault(pr => pr.PriorityName == priorityName);
        return p?.PriorityId ?? 2; // Default Medium
    }

    private TicketDto MapToDto(Ticket t)
    {
        return new TicketDto
        {
            TicketId = t.TicketId,
            TicketNumber = $"TK-{t.TicketId:D5}",
            Title = t.Title,
            Description = t.Description,
            StatusId = t.StatusId,
            StatusName = "Open",
            PriorityId = t.PriorityId,
            PriorityName = "Medium",
            SystemId = t.SystemId,
            SystemName = "",
            CategoryId = t.IssueTypeId,
            CategoryName = "",
            CreatedAt = t.CreatedAt,
            CreatedBy = t.CreatorId,
            ReporterName = "",
            AssignedTo = t.AssignedTo,
            AssigneeName = ""
        };
    }
}
