using Microsoft.EntityFrameworkCore;
using TicketingSystem.Core.Entities;
using System.Security.Cryptography;

namespace TicketingSystem.Data.Seeders;

public static class SeedData
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        // Roles
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "Employee", Description = "موظف عادي", IsActive = true },
            new Role { RoleId = 2, RoleName = "SupportSpecialist", Description = "موظف دعم فني", IsActive = true },
            new Role { RoleId = 3, RoleName = "SystemManager", Description = "مدير النظام", IsActive = true },
            new Role { RoleId = 4, RoleName = "DepartmentManager", Description = "مدير إدارة", IsActive = true },
            new Role { RoleId = 5, RoleName = "GeneralManager", Description = "مدير عام", IsActive = true },
            new Role { RoleId = 6, RoleName = "Viewer", Description = "مشاهد", IsActive = true }
        );

        // Priorities
        modelBuilder.Entity<Priority>().HasData(
            new Priority { PriorityId = 1, PriorityName = "Critical", SortOrder = 1, ColorCode = "#DC2626", IsActive = true },
            new Priority { PriorityId = 2, PriorityName = "High", SortOrder = 2, ColorCode = "#EA580C", IsActive = true },
            new Priority { PriorityId = 3, PriorityName = "Medium", SortOrder = 3, ColorCode = "#D97706", IsActive = true },
            new Priority { PriorityId = 4, PriorityName = "Low", SortOrder = 4, ColorCode = "#16A34A", IsActive = true }
        );

        // Ticket Statuses
        modelBuilder.Entity<TicketStatus>().HasData(
            new TicketStatus { StatusId = 1, StatusName = "Open", ArabicName = "مفتوحة", SortOrder = 1, IsFinal = false },
            new TicketStatus { StatusId = 2, StatusName = "Assigned", ArabicName = "مسندة", SortOrder = 2, IsFinal = false },
            new TicketStatus { StatusId = 3, StatusName = "InProgress", ArabicName = "قيد المعالجة", SortOrder = 3, IsFinal = false },
            new TicketStatus { StatusId = 4, StatusName = "Resolved", ArabicName = "محلولة - بانتظار الموافقة", SortOrder = 4, IsFinal = false },
            new TicketStatus { StatusId = 5, StatusName = "Closed", ArabicName = "مغلقة", SortOrder = 5, IsFinal = true },
            new TicketStatus { StatusId = 6, StatusName = "Reopened", ArabicName = "أُعيد فتحها", SortOrder = 6, IsFinal = false }
        );

        // Issue Types
        modelBuilder.Entity<IssueType>().HasData(
            new IssueType { IssueTypeId = 1, TypeName = "Bug", Description = "خطأ برمجي", IsActive = true },
            new IssueType { IssueTypeId = 2, TypeName = "Performance", Description = "بطء أو أداء ضعيف", IsActive = true },
            new IssueType { IssueTypeId = 3, TypeName = "ServiceDown", Description = "توقف خدمة", IsActive = true },
            new IssueType { IssueTypeId = 4, TypeName = "Security", Description = "مشكلة أمنية", IsActive = true },
            new IssueType { IssueTypeId = 5, TypeName = "FeatureRequest", Description = "طلب ميزة جديدة", IsActive = true },
            new IssueType { IssueTypeId = 6, TypeName = "Other", Description = "أخرى", IsActive = true }
        );

        // Default Admin (password: Admin@2025!)
        var adminHash = HashPassword("Admin@2025!");
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@nazaha.gov.sa",
                PasswordHash = adminHash,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
            }
        );

        // Admin = SystemManager
        modelBuilder.Entity<UserRole>().HasData(
            new UserRole { UserId = 1, RoleId = 3, AssignedBy = 1, AssignedAt = DateTime.UtcNow }
        );

        // Default Escalation Rules
        modelBuilder.Entity<EscalationRule>().HasData(
            new EscalationRule { RuleId = 1, PriorityId = 1, StepNumber = 1, WaitMinutes = 60, NotifyRole = "SupportSpecialist", Action = "تنبيه", IsActive = true, CreatedBy = 1 },
            new EscalationRule { RuleId = 2, PriorityId = 1, StepNumber = 2, WaitMinutes = 120, NotifyRole = "DepartmentManager", Action = "تصعيد", IsActive = true, CreatedBy = 1 },
            new EscalationRule { RuleId = 3, PriorityId = 1, StepNumber = 3, WaitMinutes = 240, NotifyRole = "SystemManager", Action = "تصعيد نهائي", IsActive = true, CreatedBy = 1 },
            new EscalationRule { RuleId = 4, PriorityId = 2, StepNumber = 1, WaitMinutes = 180, NotifyRole = "SupportSpecialist", Action = "تنبيه", IsActive = true, CreatedBy = 1 },
            new EscalationRule { RuleId = 5, PriorityId = 2, StepNumber = 2, WaitMinutes = 480, NotifyRole = "SystemManager", Action = "تصعيد", IsActive = true, CreatedBy = 1 },
            new EscalationRule { RuleId = 6, PriorityId = 3, StepNumber = 1, WaitMinutes = 480, NotifyRole = "SupportSpecialist", Action = "تنبيه", IsActive = true, CreatedBy = 1 },
            new EscalationRule { RuleId = 7, PriorityId = 3, StepNumber = 2, WaitMinutes = 1440, NotifyRole = "SystemManager", Action = "تصعيد", IsActive = true, CreatedBy = 1 },
            new EscalationRule { RuleId = 8, PriorityId = 4, StepNumber = 1, WaitMinutes = 1440, NotifyRole = "SupportSpecialist", Action = "تنبيه", IsActive = true, CreatedBy = 1 }
        );
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000,
            HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}:100000";
    }
}
