using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Core.Entities;

public class Ticket
{
    public int TicketId { get; set; }

    [Required, MaxLength(20)]
    public string TicketNumber { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public int CreatorId { get; set; }
    public User Creator { get; set; } = null!;

    public int CreatorDeptId { get; set; }
    public SubDepartment CreatorDepartment { get; set; } = null!;

    public int SystemId { get; set; }
    public SystemEntity System { get; set; } = null!;

    public int IssueTypeId { get; set; }
    public IssueType IssueType { get; set; } = null!;

    public int PriorityId { get; set; }
    public Priority Priority { get; set; } = null!;

    public int StatusId { get; set; }
    public TicketStatus Status { get; set; } = null!;

    public int? AssignedTo { get; set; }
    public User? AssignedToUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public DateTime? SLADueDate { get; set; }
    public bool IsSLABreached { get; set; } = false;

    public string? ResolutionSummary { get; set; }
    public int? ClosureApprovedBy { get; set; }
    public DateTime? ClosureApprovedAt { get; set; }
    public string? ReopenReason { get; set; }

    public bool IsDeleted { get; set; } = false;

    public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    public ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
    public ICollection<TicketStatusHistory> StatusHistory { get; set; } = new List<TicketStatusHistory>();
}
