namespace TicketingSystem.Core.Entities;

public class TicketStatusHistory
{
    public int HistoryId { get; set; }
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;
    public int? OldStatusId { get; set; }
    public TicketStatus? OldStatus { get; set; }
    public int NewStatusId { get; set; }
    public TicketStatus NewStatus { get; set; } = null!;
    public int ChangedBy { get; set; }
    public User ChangedByUser { get; set; } = null!;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(1000)]
    public string? Comment { get; set; }
}
