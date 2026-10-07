using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Core.Entities;

public class TicketAccessGrant
{
    public int GrantId { get; set; }
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int GrantedBy { get; set; }
    public User GrantedByUser { get; set; } = null!;
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Reason { get; set; }

    public bool IsActive { get; set; } = true;
}
