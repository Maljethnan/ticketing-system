using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Core.Entities;

public class SubDepartment
{
    public int SubDeptId { get; set; }

    [Required, MaxLength(200)]
    public string DeptName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int GeneralDeptId { get; set; }
    public GeneralDepartment GeneralDepartment { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public ICollection<User> Users { get; set; } = new List<User>();
}
