namespace TicketingSystem.Core.Entities;

public class EscalationLog
{
    public int LogId { get; set; }
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;
    public int RuleId { get; set; }
    public EscalationRule Rule { get; set; } = null!;
    public int NotifiedUserId { get; set; }
    public User NotifiedUser { get; set; } = null!;
    public DateTime EscalatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? ActionTaken { get; set; }
}
