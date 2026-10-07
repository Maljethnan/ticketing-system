using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Core.Entities;

public class RolePermission
{
    public int PermissionId { get; set; }
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    [Required, MaxLength(100)]
    public string Resource { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Action { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Scope { get; set; } = string.Empty;
}
