using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Core.Entities;

public class Priority
{
    public int PriorityId { get; set; }

    [Required, MaxLength(50)]
    public string PriorityName { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    [MaxLength(10)]
    public string? ColorCode { get; set; }

    public bool IsActive { get; set; } = true;
}
