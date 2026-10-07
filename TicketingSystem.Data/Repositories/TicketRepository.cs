using Microsoft.EntityFrameworkCore;
using TicketingSystem.Data.Context;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Data.Repositories;

public interface ITicketRepository : IRepository<Ticket>
{
    Task<string> GetNextTicketNumberAsync();
    Task<List<Ticket>> GetFilteredAsync(FilterCriteria criteria);
}

public class TicketRepository : Repository<Ticket>, ITicketRepository
{
    public TicketRepository(AppDbContext context) : base(context) { }

    public async Task<string> GetNextTicketNumberAsync()
    {
        var maxNum = await _context.Tickets
            .Where(t => !t.IsDeleted)
            .Select(t => t.TicketNumber)
            .OrderByDescending(n => n)
            .FirstOrDefaultAsync();

        int nextNum = 1;
        if (!string.IsNullOrEmpty(maxNum))
        {
            var numPart = maxNum.Substring(3);
            if (int.TryParse(numPart, out var parsed))
                nextNum = parsed + 1;
        }
        return $"TK-{nextNum:D5}";
    }

    public async Task<List<Ticket>> GetFilteredAsync(FilterCriteria criteria)
    {
        var query = _context.Tickets.Where(t => !t.IsDeleted).AsQueryable();
        if (criteria.StatusId.HasValue) query = query.Where(t => t.StatusId == criteria.StatusId.Value);
        if (criteria.SystemId.HasValue) query = query.Where(t => t.SystemId == criteria.SystemId.Value);
        if (criteria.PriorityId.HasValue) query = query.Where(t => t.PriorityId == criteria.PriorityId.Value);
        if (criteria.FromDate.HasValue) query = query.Where(t => t.CreatedAt >= criteria.FromDate.Value);
        if (criteria.ToDate.HasValue) query = query.Where(t => t.CreatedAt <= criteria.ToDate.Value);
        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var search = criteria.Search.ToLower();
            query = query.Where(t => t.Title.ToLower().Contains(search) || t.TicketNumber.ToLower().Contains(search));
        }
        return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }
}

public record FilterCriteria(string? Search, int? SystemId, int? StatusId, int? PriorityId, DateTime? FromDate, DateTime? ToDate);
