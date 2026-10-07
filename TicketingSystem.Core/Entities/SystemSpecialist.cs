namespace TicketingSystem.Core.Entities;

public class SystemSpecialist
{
    public int SystemId { get; set; }
    public SystemEntity System { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public bool IsPrimary { get; set; } = false;
    public int AddedBy { get; set; }
    public User AddedByUser { get; set; } = null!;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
