using Microsoft.Extensions.Hosting;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Data.Context;

namespace TicketingSystem.API.Services;

public class EscalationMonitorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EscalationMonitorService> _logger;

    public EscalationMonitorService(IServiceScopeFactory scopeFactory, ILogger<EscalationMonitorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ProcessEscalations(); }
            catch (Exception ex) { _logger.LogError(ex, "Error in escalation monitor"); }
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task ProcessEscalations()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();

        var openTickets = await context.Tickets.Include(t => t.Priority)
            .Where(t => t.StatusId <= 3 && !t.IsDeleted).ToListAsync();

        foreach (var ticket in openTickets)
        {
            var elapsedMinutes = (DateTime.UtcNow - ticket.CreatedAt).TotalMinutes;
            var rules = await context.EscalationRules
                .Where(r => r.PriorityId == ticket.PriorityId && r.IsActive)
                .OrderBy(r => r.StepNumber).ToListAsync();

            foreach (var rule in rules)
            {
                if (elapsedMinutes >= rule.WaitMinutes)
                {
                    var alreadyEscalated = await context.EscalationLogs
                        .AnyAsync(el => el.TicketId == ticket.TicketId && el.RuleId == rule.RuleId);
                    if (!alreadyEscalated)
                    {
                        var notifiedUserId = ResolveNotifyTarget(rule.NotifyRole, ticket);
                        if (notifiedUserId.HasValue)
                        {
                            await notificationService.NotifyAsync(notifiedUserId.Value,
                                $"⚠️ تصعيد: تذكرة {ticket.TicketNumber} - {ticket.Title}", "Escalation", ticket.TicketId);
                            context.EscalationLogs.Add(new Core.Entities.EscalationLog
                            {
                                TicketId = ticket.TicketId, RuleId = rule.RuleId,
                                NotifiedUserId = notifiedUserId.Value,
                                EscalatedAt = DateTime.UtcNow, ActionTaken = rule.Action
                            });
                            await auditService.LogAsync(0, "AutoEscalation", "Ticket", ticket.TicketId,
                                $"Escalated per rule {rule.RuleId}: {rule.Action}");
                        }
                    }
                }
            }
        }
        await context.SaveChangesAsync();
    }

    private int? ResolveNotifyTarget(string notifyRole, Core.Entities.Ticket ticket)
    {
        return notifyRole switch
        {
            "SupportSpecialist" => ticket.AssignedTo,
            _ => null
        };
    }
}
