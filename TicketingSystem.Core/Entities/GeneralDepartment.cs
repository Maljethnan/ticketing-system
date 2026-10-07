using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Core.Entities;

public class GeneralDepartment
{
    public int GeneralDeptId { get; set; }

    [Required]
    [StringLength(200)]
    public string NameAr { get; set; } = string.Empty;

    [StringLength(200)]
    public string? NameEn { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
}
