namespace TicketingSystem.Core.DTOs;

public class DashboardStats
{
    public int OpenCount { get; set; }
    public int InProgressCount { get; set; }
    public int ResolvedCount { get; set; }
    public int ClosedCount { get; set; }
}

public class StatusDistributionDto
{
    public string StatusName { get; set; } = "";
    public int Count { get; set; }
}

public class PriorityDistributionDto
{
    public string PriorityName { get; set; } = "";
    public int Count { get; set; }
}

public class SystemStatsDto
{
    public string SystemName { get; set; } = "";
    public int TicketCount { get; set; }
}
