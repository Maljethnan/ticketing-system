using TicketingSystem.Data.Seeders;
using Microsoft.EntityFrameworkCore;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Data.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<GeneralDepartment> GeneralDepartments => Set<GeneralDepartment>();
    public DbSet<SubDepartment> SubDepartments => Set<SubDepartment>();
    public DbSet<UserDepartmentAccess> UserDepartmentAccesses => Set<UserDepartmentAccess>();
    public DbSet<SystemEntity> Systems => Set<SystemEntity>();
    public DbSet<SystemSpecialist> SystemSpecialists => Set<SystemSpecialist>();
    public DbSet<IssueType> IssueTypes => Set<IssueType>();
    public DbSet<Priority> Priorities => Set<Priority>();
    public DbSet<TicketStatus> TicketStatuses => Set<TicketStatus>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketStatusHistory> TicketStatusHistories => Set<TicketStatusHistory>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();
    public DbSet<TicketAccessGrant> TicketAccessGrants => Set<TicketAccessGrant>();
    public DbSet<EscalationRule> EscalationRules => Set<EscalationRule>();
    public DbSet<EscalationLog> EscalationLogs => Set<EscalationLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<KnowledgeBase> KnowledgeBases => Set<KnowledgeBase>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>().HasKey(r => r.RoleId);
        modelBuilder.Entity<User>().HasKey(u => u.Id);
        modelBuilder.Entity<GeneralDepartment>().HasKey(g => g.GeneralDeptId);
        modelBuilder.Entity<SubDepartment>().HasKey(s => s.SubDeptId);
        modelBuilder.Entity<SystemEntity>().HasKey(s => s.SystemId);
        modelBuilder.Entity<IssueType>().HasKey(i => i.IssueTypeId);
        modelBuilder.Entity<Priority>().HasKey(p => p.PriorityId);
        modelBuilder.Entity<TicketStatus>().HasKey(s => s.StatusId);
        modelBuilder.Entity<Ticket>().HasKey(t => t.TicketId);
        modelBuilder.Entity<TicketComment>().HasKey(c => c.CommentId);
        modelBuilder.Entity<TicketAttachment>().HasKey(a => a.AttachmentId);
        modelBuilder.Entity<TicketAccessGrant>().HasKey(g => g.GrantId);
        modelBuilder.Entity<EscalationRule>().HasKey(r => r.RuleId);
        modelBuilder.Entity<EscalationLog>().HasKey(l => l.LogId);
        modelBuilder.Entity<AuditLog>().HasKey(l => l.LogId);
        modelBuilder.Entity<KnowledgeBase>().HasKey(k => k.KBId);
        modelBuilder.Entity<RolePermission>().HasKey(p => p.PermissionId);
        modelBuilder.Entity<UserDepartmentAccess>().HasKey(a => a.AccessId);
        modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
        modelBuilder.Entity<SystemSpecialist>().HasKey(ss => new { ss.SystemId, ss.UserId });
        modelBuilder.Entity<TicketStatusHistory>().HasKey(h => h.HistoryId);

        // Relationships
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany()
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .HasOne(u => u.GeneralDepartment)
            .WithMany(d => d.Users)
            .HasForeignKey(u => u.GeneralDeptId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.AssignedByUser)
            .WithMany()
            .HasForeignKey(ur => ur.AssignedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SubDepartment>()
            .HasOne(sd => sd.GeneralDepartment)
            .WithMany()
            .HasForeignKey(sd => sd.GeneralDeptId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserDepartmentAccess>()
            .HasOne(uda => uda.User)
            .WithMany()
            .HasForeignKey(uda => uda.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserDepartmentAccess>()
            .HasOne(uda => uda.SubDepartment)
            .WithMany()
            .HasForeignKey(uda => uda.SubDeptId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserDepartmentAccess>()
            .HasOne(uda => uda.GrantedByUser)
            .WithMany()
            .HasForeignKey(uda => uda.GrantedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AuditLog>()
            .HasKey(al => al.LogId);

        modelBuilder.Entity<AuditLog>()
            .HasOne(al => al.User)
            .WithMany()
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SystemSpecialist>()
            .HasKey(ss => new { ss.SystemId, ss.UserId });

        modelBuilder.Entity<SystemSpecialist>()
            .HasOne(ss => ss.User)
            .WithMany()
            .HasForeignKey(ss => ss.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SystemSpecialist>()
            .HasOne(ss => ss.AddedByUser)
            .WithMany()
            .HasForeignKey(ss => ss.AddedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Creator)
            .WithMany(u => u.CreatedTickets)
            .HasForeignKey(t => t.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.CreatorDepartment)
            .WithMany()
            .HasForeignKey(t => t.CreatorDeptId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.AssignedToUser)
            .WithMany(u => u.AssignedTickets)
            .HasForeignKey(t => t.AssignedTo)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<TicketAccessGrant>()
            .HasOne(tg => tg.User)
            .WithMany()
            .HasForeignKey(tg => tg.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TicketAccessGrant>()
            .HasOne(tg => tg.GrantedByUser)
            .WithMany()
            .HasForeignKey(tg => tg.GrantedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.System)
            .WithMany(s => s.Tickets)
            .HasForeignKey(t => t.SystemId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TicketComment>()
            .HasOne(tc => tc.ParentComment)
            .WithMany(tc => tc.Replies)
            .HasForeignKey(tc => tc.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TicketComment>()
            .HasOne(tc => tc.Sender)
            .WithMany()
            .HasForeignKey(tc => tc.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TicketComment>()
            .HasOne(tc => tc.Ticket)
            .WithMany(t => t.Comments)
            .HasForeignKey(tc => tc.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TicketStatusHistory>()
            .HasOne(h => h.Ticket)
            .WithMany(t => t.StatusHistory)
            .HasForeignKey(h => h.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TicketStatusHistory>()
            .HasOne(h => h.NewStatus)
            .WithMany()
            .HasForeignKey(h => h.NewStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TicketStatusHistory>()
            .HasOne(h => h.ChangedByUser)
            .WithMany()
            .HasForeignKey(h => h.ChangedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TicketAttachment>()
            .HasOne(a => a.Comment)
            .WithMany(c => c.Attachments)
            .HasForeignKey(a => a.CommentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TicketAttachment>()
            .HasOne(a => a.Ticket)
            .WithMany(t => t.Attachments)
            .HasForeignKey(a => a.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TicketAttachment>()
            .HasOne(a => a.UploadedBy)
            .WithMany()
            .HasForeignKey(a => a.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EscalationRule>()
            .HasKey(er => er.RuleId);

        modelBuilder.Entity<EscalationRule>()
            .HasOne(er => er.Priority)
            .WithMany()
            .HasForeignKey(er => er.PriorityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EscalationRule>()
            .HasOne(er => er.CreatedByUser)
            .WithMany()
            .HasForeignKey(er => er.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<KnowledgeBase>()
            .HasOne(kb => kb.System)
            .WithMany()
            .HasForeignKey(kb => kb.SystemId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<KnowledgeBase>()
            .HasOne(kb => kb.IssueType)
            .WithMany()
            .HasForeignKey(kb => kb.IssueTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<KnowledgeBase>()
            .HasOne(kb => kb.SourceTicket)
            .WithMany()
            .HasForeignKey(kb => kb.SourceTicketId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<KnowledgeBase>()
            .HasOne(kb => kb.CreatedByUser)
            .WithMany()
            .HasForeignKey(kb => kb.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EscalationLog>()
            .HasKey(el => el.LogId);

        modelBuilder.Entity<EscalationLog>()
            .HasOne(el => el.Ticket)
            .WithMany()
            .HasForeignKey(el => el.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EscalationLog>()
            .HasOne(el => el.Rule)
            .WithMany()
            .HasForeignKey(el => el.RuleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EscalationLog>()
            .HasOne(el => el.NotifiedUser)
            .WithMany()
            .HasForeignKey(el => el.NotifiedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EscalationRule>()
            .HasIndex(er => new { er.PriorityId, er.StepNumber })
            .IsUnique();

        // Indexes
        modelBuilder.Entity<Ticket>().HasIndex(t => t.TicketNumber).IsUnique();
        modelBuilder.Entity<Ticket>().HasIndex(t => t.StatusId);
        modelBuilder.Entity<Ticket>().HasIndex(t => t.CreatedAt);
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

        // Seed Data
        SeedData.Seed(modelBuilder);
    }
}
