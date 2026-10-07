using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Core.Entities;

public class GeneralDepartment
{
    public int GeneralDeptId { get; set; }

    [Required, MaxLength(200)]
    public string DeptName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int? ParentDeptId { get; set; }
    public GeneralDepartment? ParentDepartment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public ICollection<SubDepartment> SubDepartments { get; set; } = new List<SubDepartment>();
}
