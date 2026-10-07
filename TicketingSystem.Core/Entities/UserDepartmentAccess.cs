namespace TicketingSystem.Core.Entities;

public class UserDepartmentAccess
{
    public int AccessId { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int SubDeptId { get; set; }
    public SubDepartment SubDepartment { get; set; } = null!;
    public int GrantedBy { get; set; }
    public User GrantedByUser { get; set; } = null!;
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
