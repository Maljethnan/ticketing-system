using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Core.Entities;

public class TicketStatus
{
    public int StatusId { get; set; }

    [Required, MaxLength(50)]
    public string StatusName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string ArabicName { get; set; } = string.Empty;

    public int SortOrder { get; set; }
    public bool IsFinal { get; set; } = false;
    public bool IsActive { get; set; } = true;
}
