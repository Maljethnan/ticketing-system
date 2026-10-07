using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketingSystem.Data.Context;

namespace TicketingSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReportsController(AppDbContext context) => _context = context;

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var openCount = await _context.Tickets.CountAsync(t => t.StatusId <= 3 && !t.IsDeleted);
        var inProgressCount = await _context.Tickets.CountAsync(t => t.StatusId == 3 && !t.IsDeleted);
        var closedThisMonth = await _context.Tickets.CountAsync(t => t.StatusId == 5 && t.ClosedAt >= monthStart);
        var slaBreached = await _context.Tickets.CountAsync(t => t.IsSLABreached && t.StatusId <= 3);

        var avgResolutionHours = await _context.Tickets
            .Where(t => t.StatusId == 5 && t.ClosedAt.HasValue)
            .AverageAsync(t => (t.ClosedAt!.Value - t.CreatedAt).TotalHours);

        var topSystem = await _context.Tickets.Include(t => t.System)
            .Where(t => !t.IsDeleted)
            .GroupBy(t => t.SystemId)
            .OrderByDescending(g => g.Count())
            .Select(g => g.First().System.SystemName)
            .FirstOrDefaultAsync();

        var bestSpecialist = await _context.Tickets
            .Where(t => t.StatusId == 5 && t.AssignedTo.HasValue)
            .GroupBy(t => t.AssignedTo.Value)
            .Join(_context.Users, g => g.Key, u => u.UserId, (g, u) => new
            { UserName = u.UserName, AvgHours = g.Average(t => (t.ClosedAt!.Value - t.CreatedAt).TotalHours) })
            .OrderBy(x => x.AvgHours)
            .Select(x => x.UserName)
            .FirstOrDefaultAsync();

        return Ok(new
        {
            OpenCount = openCount, InProgressCount = inProgressCount,
            ClosedCount = closedThisMonth, SLABreachedCount = slaBreached,
            AvgResolutionHours = Math.Round(avgResolutionHours, 1),
            TopAffectedSystem = topSystem ?? "—", BestPerformer = bestSpecialist ?? "—"
        });
    }

    [HttpGet("status-distribution")]
    public async Task<IActionResult> GetStatusDistribution([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var query = _context.Tickets.Where(t => !t.IsDeleted);
        if (from.HasValue) query = query.Where(t => t.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(t => t.CreatedAt <= to.Value);
        return Ok(await query.Include(t => t.Status)
            .GroupBy(t => t.StatusId)
            .Select(g => new { StatusId = g.Key, StatusName = g.First().Status.ArabicName, Count = g.Count() })
            .ToListAsync());
    }

    [HttpGet("priority-distribution")]
    public async Task<IActionResult> GetPriorityDistribution([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var query = _context.Tickets.Where(t => !t.IsDeleted);
        if (from.HasValue) query = query.Where(t => t.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(t => t.CreatedAt <= to.Value);
        return Ok(await query.Include(t => t.Priority)
            .GroupBy(t => t.PriorityId)
            .Select(g => new { PriorityId = g.Key, PriorityName = g.First().Priority.PriorityName, ColorCode = g.First().Priority.ColorCode, Count = g.Count() })
            .ToListAsync());
    }

    [HttpGet("by-system")]
    public async Task<IActionResult> GetBySystem([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var query = _context.Tickets.Where(t => !t.IsDeleted);
        if (from.HasValue) query = query.Where(t => t.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(t => t.CreatedAt <= to.Value);
        return Ok(await query.Include(t => t.System)
            .GroupBy(t => t.SystemId)
            .Select(g => new
            {
                SystemId = g.Key, SystemName = g.First().System.SystemName,
                TotalCount = g.Count(), OpenCount = g.Count(t => t.StatusId <= 3),
                ClosedCount = g.Count(t => t.StatusId == 5),
                AvgResolutionHours = g.Where(t => t.StatusId == 5 && t.ClosedAt.HasValue).Average(t => (t.ClosedAt!.Value - t.CreatedAt).TotalHours)
            }).OrderByDescending(r => r.TotalCount).ToListAsync());
    }

    [HttpGet("by-department")]
    public async Task<IActionResult> GetByDepartment([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var query = _context.Tickets.Where(t => !t.IsDeleted);
        if (from.HasValue) query = query.Where(t => t.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(t => t.CreatedAt <= to.Value);
        return Ok(await query.Include(t => t.CreatorDepartment)
            .GroupBy(t => t.CreatorDeptId)
            .Select(g => new
            {
                DeptId = g.Key, DeptName = g.First().CreatorDepartment.DeptName,
                TotalCount = g.Count(), OpenCount = g.Count(t => t.StatusId <= 3),
                ClosedCount = g.Count(t => t.StatusId == 5),
                AvgResolutionHours = g.Where(t => t.StatusId == 5 && t.ClosedAt.HasValue).Average(t => (t.ClosedAt!.Value - t.CreatedAt).TotalHours)
            }).OrderByDescending(r => r.TotalCount).ToListAsync());
    }

    [HttpGet("support-performance")]
    public async Task<IActionResult> GetSupportPerformance([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var query = _context.Tickets.Where(t => t.AssignedTo.HasValue && !t.IsDeleted);
        if (from.HasValue) query = query.Where(t => t.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(t => t.CreatedAt <= to.Value);
        return Ok(await query.GroupBy(t => t.AssignedTo.Value)
            .Join(_context.Users, g => g.Key, u => u.UserId, (g, u) => new
            {
                SpecialistName = u.UserName, TotalAssigned = g.Count(),
                ClosedCount = g.Count(t => t.StatusId == 5), OpenCount = g.Count(t => t.StatusId <= 3),
                AvgResolutionHours = g.Where(t => t.StatusId == 5 && t.ClosedAt.HasValue).Average(t => (t.ClosedAt!.Value - t.CreatedAt).TotalHours),
                SLABreachedCount = g.Count(t => t.IsSLABreached)
            }).OrderByDescending(p => p.ClosedCount).ToListAsync());
    }

    [HttpGet("daily-trend")]
    public async Task<IActionResult> GetDailyTrend([FromQuery] int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        return Ok(await _context.Tickets
            .Where(t => t.CreatedAt >= startDate && !t.IsDeleted)
            .GroupBy(t => new { t.CreatedAt.Date, t.StatusId })
            .Select(g => new { Date = g.Key.Date, StatusId = g.Key.StatusId, Count = g.Count() })
            .OrderBy(t => t.Date).ToListAsync());
    }

    [HttpGet("issue-types")]
    public async Task<IActionResult> GetIssueTypes([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var query = _context.Tickets.Where(t => !t.IsDeleted);
        if (from.HasValue) query = query.Where(t => t.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(t => t.CreatedAt <= to.Value);
        return Ok(await query.Include(t => t.IssueType)
            .GroupBy(t => t.IssueTypeId)
            .Select(g => new { IssueTypeId = g.Key, TypeName = g.First().IssueType.TypeName, Description = g.First().IssueType.Description, Count = g.Count() })
            .OrderByDescending(r => r.Count).ToListAsync());
    }

    [HttpGet("export-csv")]
    public async Task<IActionResult> ExportCsv([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var query = _context.Tickets.Include(t => t.System).Include(t => t.Priority).Include(t => t.Status)
            .Include(t => t.IssueType).Include(t => t.Creator).Include(t => t.AssignedToUser)
            .Where(t => !t.IsDeleted);
        if (from.HasValue) query = query.Where(t => t.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(t => t.CreatedAt <= to.Value);

        var tickets = await query.ToListAsync();
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("رقم التذكرة,العنوان,النظام,نوع المشكلة,الأولوية,الحالة,المنشئ,المختص,تاريخ الإنشاء,تاريخ الإغلاق,مدة المعالجة");

        foreach (var t in tickets)
        {
            var hours = t.ClosedAt.HasValue ? ((t.ClosedAt.Value - t.CreatedAt).TotalHours).ToString("F1") : "";
            csv.AppendLine($"{EscapeCsv(t.TicketNumber)},{EscapeCsv(t.Title)},{EscapeCsv(t.System?.SystemName ?? "")},{EscapeCsv(t.IssueType?.TypeName ?? "")},{EscapeCsv(t.Priority?.PriorityName ?? "")},{EscapeCsv(t.Status?.ArabicName ?? "")},{EscapeCsv(t.Creator?.UserName ?? "")},{EscapeCsv(t.AssignedToUser?.UserName ?? "")},{t.CreatedAt:yyyy-MM-dd HH:mm},{t.ClosedAt?.ToString("yyyy-MM-dd HH:mm")},{hours}");
        }

        Response.Headers["Content-Disposition"] = $"attachment; filename=tickets_{DateTime.UtcNow:yyyyMMdd}.csv";
        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv");
    }

    private string EscapeCsv(string value) => value.Contains(',') || value.Contains('"') ? $""{value.Replace(""", """")}"" : value;
}
