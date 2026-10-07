using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Core.Entities;

public class TicketComment
{
    public int CommentId { get; set; }
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public int SenderId { get; set; }
    public User Sender { get; set; } = null!;

    [Required]
    public string Content { get; set; } = string.Empty;

    public int? ParentCommentId { get; set; }
    public TicketComment? ParentComment { get; set; }
    public ICollection<TicketComment> Replies { get; set; } = new List<TicketComment>();

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsEdited { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public int? DeletedById { get; set; }
    public bool IsInternal { get; set; } = false;

    public ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
}
