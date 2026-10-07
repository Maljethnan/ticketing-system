using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Data.Context;

namespace TicketingSystem.API.Controllers;

[ApiController]
[Route("api/tickets/{ticketId}/[controller]")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPermissionService _permissionService;
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;

    public CommentsController(AppDbContext context, IPermissionService permissionService,
        INotificationService notificationService, IAuditService auditService)
    {
        _context = context;
        _permissionService = permissionService;
        _notificationService = notificationService;
        _auditService = auditService;
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

    [HttpGet]
    public async Task<IActionResult> GetComments(int ticketId)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null) return NotFound();

        var user = await _context.Users.FindAsync(GetCurrentUserId());
        if (user == null || !_permissionService.CanViewTicket(user, ticket)) return Forbid();

        var comments = await _context.TicketComments
            .Include(c => c.Sender)
            .Include(c => c.Attachments)
            .Where(c => c.TicketId == ticketId && c.DeletedAt == null)
            .OrderBy(c => c.SentAt)
            .ToListAsync();

        return Ok(comments);
    }

    [HttpPost]
    public async Task<IActionResult> AddComment(int ticketId, [FromBody] AddCommentRequest request)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null) return NotFound();

        var user = await _context.Users.FindAsync(GetCurrentUserId());
        if (user == null || !_permissionService.CanCommentOnTicket(user, ticket)) return Forbid();

        var comment = new TicketComment
        {
            TicketId = ticketId,
            SenderId = GetCurrentUserId(),
            Content = System.Net.WebUtility.HtmlEncode(request.Content),
            ParentCommentId = request.ParentCommentId,
            SentAt = DateTime.UtcNow,
            IsInternal = request.IsInternal ?? false
        };

        _context.TicketComments.Add(comment);
        await _context.SaveChangesAsync();

        // Notify others
        if (ticket.CreatorId != GetCurrentUserId())
            await _notificationService.NotifyAsync(ticket.CreatorId, $"تعليق جديد في تذكرة {ticket.TicketNumber}", "NewComment", ticketId);
        if (ticket.AssignedTo.HasValue && ticket.AssignedTo.Value != GetCurrentUserId())
            await _notificationService.NotifyAsync(ticket.AssignedTo.Value, $"تعليق جديد في تذكرة {ticket.TicketNumber}", "NewComment", ticketId);

        await _auditService.LogAsync(GetCurrentUserId(), "AddComment", "Ticket", ticketId, $"Comment #{comment.CommentId}");
        return CreatedAtAction(nameof(GetComments), new { ticketId }, comment);
    }

    [HttpPut("{commentId}")]
    public async Task<IActionResult> EditComment(int ticketId, int commentId, [FromBody] EditCommentRequest request)
    {
        var comment = await _context.TicketComments
            .FirstOrDefaultAsync(c => c.CommentId == commentId && c.TicketId == ticketId);
        if (comment == null) return NotFound();
        if (comment.SenderId != GetCurrentUserId()) return Forbid();

        comment.Content = System.Net.WebUtility.HtmlEncode(request.Content);
        comment.IsEdited = true;
        comment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(comment);
    }

    [HttpDelete("{commentId}")]
    public async Task<IActionResult> DeleteComment(int ticketId, int commentId)
    {
        var comment = await _context.TicketComments
            .FirstOrDefaultAsync(c => c.CommentId == commentId && c.TicketId == ticketId);
        if (comment == null) return NotFound();
        if (comment.SenderId != GetCurrentUserId() && !User.IsInRole("SystemManager")) return Forbid();

        comment.DeletedAt = DateTime.UtcNow;
        comment.DeletedById = GetCurrentUserId();
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
