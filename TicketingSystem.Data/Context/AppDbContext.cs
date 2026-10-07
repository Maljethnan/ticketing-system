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

        // Relationships
        modelBuilder.Entity<User>()
            .HasOne(u => u.SubDepartment)
            .WithMany(d => d.Users)
            .HasForeignKey(u => u.SubDeptId)
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
            .WithMany(gd => gd.SubDepartments)
            .HasForeignKey(sd => sd.GeneralDeptId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SystemSpecialist>()
            .HasKey(ss => new { ss.SystemId, ss.UserId });

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Creator)
            .WithMany()
            .HasForeignKey(t => t.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.AssignedToUser)
            .WithMany()
            .HasForeignKey(t => t.AssignedTo)
            .OnDelete(DeleteBehavior.SetNull);

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
            .HasOne(tc => tc.Ticket)
            .WithMany(t => t.Comments)
            .HasForeignKey(tc => tc.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TicketAttachment>()
            .HasOne(a => a.Comment)
            .WithMany(c => c.Attachments)
            .HasForeignKey(a => a.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TicketAttachment>()
            .HasOne(a => a.Ticket)
            .WithMany(t => t.Attachments)
            .HasForeignKey(a => a.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EscalationRule>()
            .HasIndex(er => new { er.PriorityId, er.StepNumber })
            .IsUnique();

        // Indexes
        modelBuilder.Entity<Ticket>().HasIndex(t => t.TicketNumber).IsUnique();
        modelBuilder.Entity<Ticket>().HasIndex(t => t.StatusId);
        modelBuilder.Entity<Ticket>().HasIndex(t => t.CreatedAt);
        modelBuilder.Entity<User>().HasIndex(u => u.UserName).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

        // Seed Data
        SeedData.Seed(modelBuilder);
    }
}
