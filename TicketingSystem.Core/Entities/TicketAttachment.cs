using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Core.Entities;

public class TicketAttachment
{
    public int AttachmentId { get; set; }
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public int? CommentId { get; set; }
    public TicketComment? Comment { get; set; }

    public int UploadedById { get; set; }
    public User UploadedBy { get; set; } = null!;

    [Required, MaxLength(300)]
    public string FileName { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }
    public string? MimeType { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
    public int? DeletedById { get; set; }
}
