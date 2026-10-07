using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Core.Entities;

public class AuditLog
{
    public long LogId { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Action { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Entity { get; set; }

    public int? EntityId { get; set; }

    [MaxLength(int.MaxValue)]
    public string? Details { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
}
