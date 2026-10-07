using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Services;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Data.Context;

namespace TicketingSystem.API.Controllers;

[ApiController]
[Route("api/tickets/{ticketId}/[controller]")]
[Authorize]
public class AttachmentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IPermissionService _permissionService;
    private readonly IConfiguration _config;

    public AttachmentsController(AppDbContext context, IFileStorageService fileStorage,
        IPermissionService permissionService, IConfiguration config)
    {
        _context = context;
        _fileStorage = fileStorage;
        _permissionService = permissionService;
        _config = config;
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

    private byte[] ReadFileBytes(IFormFile file)
    {
        using var ms = new MemoryStream();
        file.CopyTo(ms);
        return ms.ToArray();
    }

    [HttpPost]
    public async Task<IActionResult> Upload(int ticketId, IFormFile file)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null) return NotFound();

        var user = await _context.Users.FindAsync(GetCurrentUserId());
        if (user == null || !_permissionService.CanViewTicket(user, ticket)) return Forbid();

        var sizeStr = _config["FileStorage:MaxFileSizeMB"];
        if (string.IsNullOrEmpty(sizeStr)) return BadRequest(new { message = "إعداد حجم الملف غير محدد" });
        var maxBytes = long.Parse(sizeStr) * 1024 * 1024;
        if (file.Length > maxBytes)
            return BadRequest(new { message = "حجم الملف يتجاوز الحد الأقصى (10 MB)" });

        var allowedExtensions = _config.GetSection("FileStorage:AllowedExtensions").Get<string[]>() ?? Array.Empty<string>();
        var extension = Path.GetExtension(file.FileName)?.ToLower() ?? "";
        if (!allowedExtensions.Contains(extension))
            return BadRequest(new { message = "نوع الملف غير مسموح به" });

        var blockedExtensions = new[] { ".exe", ".bat", ".ps1", ".vbs", ".js", ".dll", ".msi", ".scr" };
        if (blockedExtensions.Contains(extension))
            return BadRequest(new { message = "هذا النوع من الملفات محظور لأسباب أمنية" });

        var filePath = await _fileStorage.SaveAsync(ReadFileBytes(file), file.FileName, "attachments");

        var attachment = new TicketAttachment
        {
            TicketId = ticketId,
            UploadedById = GetCurrentUserId(),
            FileName = file.FileName,
            FilePath = filePath,
            FileSizeBytes = file.Length,
            MimeType = file.ContentType,
            UploadedAt = DateTime.UtcNow
        };

        _context.TicketAttachments.Add(attachment);
        await _context.SaveChangesAsync();
        return Ok(attachment);
    }

    [HttpPost("with-comment")]
    public async Task<IActionResult> UploadWithComment(int ticketId, IFormFile file, [FromForm] string content, [FromForm] int? parentCommentId)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null) return NotFound();

        var user = await _context.Users.FindAsync(GetCurrentUserId());
        if (user == null || !_permissionService.CanCommentOnTicket(user, ticket)) return Forbid();

        var filePath = await _fileStorage.SaveAsync(ReadFileBytes(file), file.FileName, "attachments");

        var comment = new TicketComment
        {
            TicketId = ticketId,
            SenderId = GetCurrentUserId(),
            Content = System.Net.WebUtility.HtmlEncode(content),
            ParentCommentId = parentCommentId,
            SentAt = DateTime.UtcNow
        };

        _context.TicketComments.Add(comment);
        await _context.SaveChangesAsync();

        var attachment = new TicketAttachment
        {
            TicketId = ticketId,
            CommentId = comment.CommentId,
            UploadedById = GetCurrentUserId(),
            FileName = file.FileName,
            FilePath = filePath,
            FileSizeBytes = file.Length,
            MimeType = file.ContentType,
            UploadedAt = DateTime.UtcNow
        };

        _context.TicketAttachments.Add(attachment);
        await _context.SaveChangesAsync();
        return Ok(new { comment, attachment });
    }

    [HttpGet]
    public async Task<IActionResult> List(int ticketId)
    {
        return Ok(await _context.TicketAttachments
            .Where(a => a.TicketId == ticketId && a.DeletedAt == null)
            .ToListAsync());
    }

    [HttpGet("{attachmentId}/download")]
    public async Task<IActionResult> Download(int ticketId, int attachmentId)
    {
        var attachment = await _context.TicketAttachments
            .FirstOrDefaultAsync(a => a.AttachmentId == attachmentId && a.TicketId == ticketId);
        if (attachment == null) return NotFound();

        var user = await _context.Users.FindAsync(GetCurrentUserId());
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (user == null || ticket == null || !_permissionService.CanViewTicket(user, ticket))
            return Forbid();

        return PhysicalFile(attachment.FilePath, attachment.MimeType ?? "application/octet-stream", attachment.FileName);
    }

    [HttpDelete("{attachmentId}")]
    public async Task<IActionResult> Delete(int ticketId, int attachmentId)
    {
        var attachment = await _context.TicketAttachments
            .FirstOrDefaultAsync(a => a.AttachmentId == attachmentId && a.TicketId == ticketId);
        if (attachment == null) return NotFound();
        if (attachment.UploadedById != GetCurrentUserId() && !User.IsInRole("SystemManager")) return Forbid();

        attachment.DeletedAt = DateTime.UtcNow;
        attachment.DeletedById = GetCurrentUserId();
        _fileStorage.DeleteAsync(attachment.FilePath);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
