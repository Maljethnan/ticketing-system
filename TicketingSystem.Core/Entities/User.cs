using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketingSystem.Core.Entities;

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [StringLength(200)]
    public string? FullName { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? PasswordHash { get; set; }

    public int RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;

    public int? GeneralDeptId { get; set; }
    public virtual GeneralDepartment? GeneralDepartment { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// هل المستخدم مسجل من Active Directory؟
    /// إذا كان true → لا يحتاج كلمة مرور محلية
    /// </summary>
    public bool IsAdUser { get; set; } = false;

    /// <summary>
    /// هل يجب على المستخدم تغيير كلمة المرور عند أول دخول؟
    /// يفعّل فقط عند إنشاء مستخدم محلي بواسطة المدير العام
    /// </summary>
    public bool MustChangePassword { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public virtual ICollection<Ticket> CreatedTickets { get; set; } = new List<Ticket>();
    public virtual ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();
}
