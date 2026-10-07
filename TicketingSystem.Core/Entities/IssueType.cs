using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Core.Entities;

public class IssueType
{
    public int IssueTypeId { get; set; }

    [Required, MaxLength(100)]
    public string TypeName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
