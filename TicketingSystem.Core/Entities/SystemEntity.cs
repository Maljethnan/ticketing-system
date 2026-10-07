using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Core.Entities;

public class SystemEntity
{
    public int SystemId { get; set; }

    [Required, MaxLength(200)]
    public string SystemName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public ICollection<SystemSpecialist> Specialists { get; set; } = new List<SystemSpecialist>();
}
