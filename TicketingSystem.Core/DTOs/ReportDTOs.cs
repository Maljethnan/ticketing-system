namespace TicketingSystem.Core.DTOs;

public record DashboardStats(int OpenCount, int InProgressCount, int ClosedCount, int SLABreachedCount, double AvgResolutionHours, string TopAffectedSystem, string BestPerformer);
public record StatusDistributionDto(int StatusId, string StatusName, int Count);
public record PriorityDistributionDto(int PriorityId, string PriorityName, string ColorCode, int Count);
public record SystemReportDto(int SystemId, string SystemName, int TotalCount, int OpenCount, int ClosedCount, double AvgResolutionHours);
public record DepartmentReportDto(int DeptId, string DeptName, int TotalCount, int OpenCount, int ClosedCount, double AvgResolutionHours);
public record SupportPerformanceDto(string SpecialistName, int TotalAssigned, int ClosedCount, int OpenCount, double AvgResolutionHours, int SLABreachedCount);
public record DailyTrendDto(DateTime Date, int StatusId, int Count);
public record IssueTypeReportDto(int IssueTypeId, string TypeName, string Description, int Count);
public record SystemDto(int SystemId, string SystemName, string Category);
public record IssueTypeDto(int IssueTypeId, string TypeName, string Description);
public record PriorityDto(int PriorityId, string PriorityName, string ColorCode);
