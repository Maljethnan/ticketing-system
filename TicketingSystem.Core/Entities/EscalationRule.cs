using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Core.Entities;

public class EscalationRule
{
    public int RuleId { get; set; }
    public int PriorityId { get; set; }
    public Priority Priority { get; set; } = null!;
    public int StepNumber { get; set; }
    public int WaitMinutes { get; set; }

    [Required, MaxLength(100)]
    public string NotifyRole { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public int CreatedBy { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
