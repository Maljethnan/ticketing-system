using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Core.Entities;

public class User
{
    public int UserId { get; set; }

    [Required, MaxLength(100)]
    public string UserName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Email { get; set; }

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    public int? SubDeptId { get; set; }
    public SubDepartment? SubDepartment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsLocked { get; set; } = false;

    // MFA
    public string? TfaSecret { get; set; }
    public bool TfaEnabled { get; set; } = false;

    // Password Policy
    public DateTime? PasswordChangedAt { get; set; }
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockedUntil { get; set; }

    // Navigation
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
