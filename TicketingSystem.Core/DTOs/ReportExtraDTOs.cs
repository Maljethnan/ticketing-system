namespace TicketingSystem.Core.DTOs;

public class SystemReportDto
{
    public string SystemName { get; set; } = "";
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
}

public class DepartmentReportDto
{
    public string DepartmentName { get; set; } = "";
    public int TotalTickets { get; set; }
    public int AvgProcessingHours { get; set; }
}

public class SupportPerformanceDto
{
    public string SpecialistName { get; set; } = "";
    public int HandledCount { get; set; }
    public double AvgResolutionHours { get; set; }
}

public class IssueTypeReportDto
{
    public string IssueTypeName { get; set; } = "";
    public int Count { get; set; }
}
