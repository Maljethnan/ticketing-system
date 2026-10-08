using Microsoft.EntityFrameworkCore;
using TicketingSystem.Core.DTOs;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Data.Context;

namespace TicketingSystem.API.Services;

public class TicketService : ITicketService
{
    private readonly AppDbContext _db;

    public TicketService(AppDbContext db) => _db = db;

    public async Task<int> CreateTicketAsync(CreateTicketDto dto, int userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("عنوان التذكرة مطلوب", nameof(dto));

        if (dto.SystemId <= 0)
            throw new ArgumentException("يجب اختيار نظام صالح", nameof(dto));

        if (dto.CategoryId <= 0)
            throw new ArgumentException("يجب اختيار نوع المشكلة", nameof(dto));

        var userExists = await _db.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
            throw new InvalidOperationException("المستخدم غير موجود");

        var priorityId = await ResolvePriorityIdAsync(dto.Priority);
        var departmentId = await ResolveCreatorDepartmentIdAsync(userId);

        var ticket = new Ticket
        {
            TicketNumber = $"TK-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
            Title = dto.Title,
            Description = dto.Description,
            SystemId = dto.SystemId,
            IssueTypeId = dto.CategoryId,
            PriorityId = priorityId,
            CreatorId = userId,
            CreatorDeptId = departmentId,
            StatusId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();
        return ticket.TicketId;
    }

    public async Task<TicketDto?> GetTicketAsync(int ticketId, int userId)
    {
        var ticket = await _db.Tickets
            .Include(t => t.System)
            .Include(t => t.IssueType)
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .Include(t => t.Creator)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.TicketId == ticketId);

        if (ticket == null) return null;
        return MapToDto(ticket);
    }

    public async Task<List<TicketDto>> GetTicketsAsync(FilterRequest filter, int userId)
    {
        var query = _db.Tickets
            .Include(t => t.System)
            .Include(t => t.IssueType)
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .Include(t => t.Creator)
            .Include(t => t.AssignedToUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(t => t.Title.Contains(filter.Search) || t.Description.Contains(filter.Search));
        if (filter.StatusId.HasValue)
            query = query.Where(t => t.StatusId == filter.StatusId.Value);
        if (filter.PriorityId.HasValue)
            query = query.Where(t => t.PriorityId == filter.PriorityId.Value);
        if (filter.SystemId.HasValue)
            query = query.Where(t => t.SystemId == filter.SystemId.Value);

        if (filter.FromDate.HasValue)
            query = query.Where(t => t.CreatedAt >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            query = query.Where(t => t.CreatedAt <= filter.ToDate.Value);

        var tickets = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        return tickets.Select(MapToDto).ToList();
    }

    public async Task<List<TicketDto>> GetMyTicketsAsync(int userId, FilterRequest filter)
    {
        var query = _db.Tickets
            .Include(t => t.System)
            .Include(t => t.IssueType)
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .Include(t => t.Creator)
            .Include(t => t.AssignedToUser)
            .Where(t => t.CreatorId == userId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(t => t.Title.Contains(filter.Search) || t.Description.Contains(filter.Search));

        var tickets = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        return tickets.Select(MapToDto).ToList();
    }

    public async Task UpdateTicketStatusAsync(int ticketId, StatusUpdateRequest request, int userId)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.TicketId == ticketId);
        if (ticket == null) return;

        ticket.StatusId = request.StatusId;
        ticket.UpdatedAt = DateTime.UtcNow;

        if (request.StatusId == 5)
            ticket.ClosedAt ??= DateTime.UtcNow;
        else if (request.StatusId != 5)
            ticket.ClosedAt = null;

        await _db.SaveChangesAsync();
    }

    public async Task CloseTicketAsync(int ticketId, CloseRequest request, int userId)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.TicketId == ticketId);
        if (ticket == null) return;

        ticket.StatusId = 4;
        ticket.ResolutionSummary = request.ResolutionSummary ?? request.ResolutionNote;
        ticket.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task ApproveClosureAsync(int ticketId, int userId)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.TicketId == ticketId);
        if (ticket == null) return;

        ticket.StatusId = 5;
        ticket.ClosureApprovedBy = userId;
        ticket.ClosureApprovedAt = DateTime.UtcNow;
        ticket.ClosedAt ??= DateTime.UtcNow;
        ticket.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task ReopenTicketAsync(int ticketId, ReopenRequest request, int userId)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.TicketId == ticketId);
        if (ticket == null) return;

        ticket.StatusId = 6;
        ticket.ReopenReason = request.Reason;
        ticket.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task GrantAccessAsync(int ticketId, GrantAccessRequest request, int userId)
    {
        var targetUserId = request.GranteeUserId ?? request.UserId;
        if (targetUserId <= 0)
            return;

        var userExists = await _db.Users.AnyAsync(u => u.Id == targetUserId);
        if (!userExists)
            return;

        var alreadyGranted = await _db.TicketAccessGrants.AnyAsync(g =>
            g.TicketId == ticketId && g.UserId == targetUserId && g.IsActive);

        if (alreadyGranted)
            return;

        var grant = new TicketAccessGrant
        {
            TicketId = ticketId,
            UserId = targetUserId,
            GrantedBy = userId,
            Reason = request.Reason,
            IsActive = true,
            GrantedAt = DateTime.UtcNow
        };

        _db.TicketAccessGrants.Add(grant);
        await _db.SaveChangesAsync();
    }

    private async Task<int> ResolvePriorityIdAsync(string priorityName)
    {
        if (string.IsNullOrWhiteSpace(priorityName))
            return 2;

        var priority = await _db.Priorities
            .FirstOrDefaultAsync(p => p.PriorityName.Equals(priorityName, StringComparison.OrdinalIgnoreCase));

        return priority?.PriorityId ?? 2;
    }

    private async Task<int> ResolveCreatorDepartmentIdAsync(int userId)
    {
        var deptId = await _db.UserDepartmentAccesses
            .Where(uda => uda.UserId == userId && uda.IsActive)
            .Select(uda => uda.SubDeptId)
            .FirstOrDefaultAsync();

        if (deptId != 0)
            return deptId;

        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.GeneralDeptId.HasValue == true && user.GeneralDeptId.Value > 0)
        {
            var fallback = await _db.SubDepartments
                .Where(sd => sd.GeneralDeptId == user.GeneralDeptId.Value)
                .Select(sd => sd.SubDeptId)
                .FirstOrDefaultAsync();

            if (fallback != 0)
                return fallback;
        }

        var firstSubDepartment = await _db.SubDepartments
            .Select(sd => sd.SubDeptId)
            .FirstOrDefaultAsync();

        if (firstSubDepartment != 0)
            return firstSubDepartment;

        throw new InvalidOperationException("لا توجد أقسام فرعية متاحة لملء بيانات التذكرة");
    }

    private static TicketDto MapToDto(Ticket t)
    {
        return new TicketDto
        {
            TicketId = t.TicketId,
            TicketNumber = string.IsNullOrWhiteSpace(t.TicketNumber) ? $"TK-{t.TicketId:D5}" : t.TicketNumber,
            Title = t.Title,
            Description = t.Description,
            StatusId = t.StatusId,
            StatusName = t.Status?.ArabicName ?? t.Status?.StatusName ?? "",
            PriorityId = t.PriorityId,
            PriorityName = t.Priority?.PriorityName ?? "",
            SystemId = t.SystemId,
            SystemName = t.System?.SystemName ?? "",
            CategoryId = t.IssueTypeId,
            CategoryName = t.IssueType?.TypeName ?? "",
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
            CreatedBy = t.CreatorId,
            ReporterName = t.Creator?.FullName ?? t.Creator?.Username ?? "",
            AssignedTo = t.AssignedTo,
            AssigneeName = t.AssignedToUser?.FullName ?? t.AssignedToUser?.Username ?? "",
            Attachments = t.Attachments?.Select(a => new AttachmentDto
            {
                Id = a.AttachmentId,
                FileName = a.FileName,
                FileSizeBytes = a.FileSizeBytes,
                UploadedAt = a.UploadedAt
            }).ToList()
        };
    }
}
