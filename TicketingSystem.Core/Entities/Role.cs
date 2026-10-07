using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Core.Entities;

public class Role
{
    public int RoleId { get; set; }

    [Required, MaxLength(100)]
    public string RoleName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
