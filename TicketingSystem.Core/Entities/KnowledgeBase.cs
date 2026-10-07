using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Core.Entities;

public class KnowledgeBase
{
    public int KBId { get; set; }

    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string ProblemDesc { get; set; } = string.Empty;

    [Required]
    public string SolutionDesc { get; set; } = string.Empty;

    public int SystemId { get; set; }
    public SystemEntity System { get; set; } = null!;

    public int IssueTypeId { get; set; }
    public IssueType IssueType { get; set; } = null!;

    public int? SourceTicketId { get; set; }
    public Ticket? SourceTicket { get; set; }

    public int CreatedBy { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int ViewsCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}
