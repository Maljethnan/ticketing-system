using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Data.Context;

namespace TicketingSystem.API.Services;

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _context;
    public PermissionService(AppDbContext context) => _context = context;

    public bool HasPermission(User user, string resource, string action)
    {
        if (IsSystemOwner(user)) return true;
        return _context.RolePermissions.Any(rp => rp.Resource == resource && rp.Action == action &&
            _context.UserRoles.Any(ur => ur.UserId == user.UserId && ur.RoleId == rp.RoleId));
    }

    public bool CanViewTicket(User user, Ticket ticket)
    {
        if (IsSystemOwner(user)) return true;
        if (user.UserId == ticket.CreatorId) return true;
        if (ticket.AssignedTo == user.UserId) return true;
        if (_context.TicketAccessGrants.Any(g => g.TicketId == ticket.TicketId && g.UserId == user.UserId && g.IsActive)) return true;
        if (CanAccessDepartment(user, ticket.CreatorDeptId)) return true;
        if (IsGeneralManager(user) && IsUnderGeneralDept(user, ticket.CreatorDeptId)) return true;
        return false;
    }

    public bool CanEditTicket(User user, Ticket ticket)
    {
        if (IsSystemOwner(user)) return true;
        if (ticket.AssignedTo == user.UserId) return true;
        if (user.UserId == ticket.CreatorId) return true;
        return false;
    }

    public bool CanCloseTicket(User user, Ticket ticket)
    {
        if (IsSystemOwner(user)) return true;
        if (ticket.AssignedTo == user.UserId) return true;
        return false;
    }

    public bool CanApproveClosure(User user, Ticket ticket)
    {
        if (IsSystemOwner(user)) return true;
        if (user.UserId == ticket.CreatorId) return true;
        if (user.SubDeptId == ticket.CreatorDeptId && HasRole(user, "DepartmentManager")) return true;
        return false;
    }

    public bool CanCommentOnTicket(User user, Ticket ticket) => CanViewTicket(user, ticket);
    public bool CanViewAllTickets(User user) => IsSystemOwner(user) || IsGeneralManager(user);
    public bool CanManageSettings(User user) => IsSystemOwner(user);

    public List<int> GetAccessibleDepartmentIds(User user)
    {
        var deptIds = new HashSet<int>();
        if (user.SubDeptId.HasValue) deptIds.Add(user.SubDeptId.Value);
        deptIds.UnionWith(_context.UserDepartmentAccesses.Where(uda => uda.UserId == user.UserId && uda.IsActive).Select(uda => uda.SubDeptId).ToList());
        if (IsGeneralManager(user))
        {
            var generalDeptId = _context.SubDepartments.Where(sd => sd.SubDeptId == user.SubDeptId).Select(sd => sd.GeneralDeptId).FirstOrDefault();
            if (generalDeptId != default)
                deptIds.UnionWith(_context.SubDepartments.Where(sd => sd.GeneralDeptId == generalDeptId).Select(sd => sd.SubDeptId).ToList());
        }
        return deptIds.ToList();
    }

    public List<int> GetAccessibleSystemIds(User user)
    {
        if (IsSystemOwner(user)) return _context.Systems.Where(s => s.IsActive).Select(s => s.SystemId).ToList();
        return _context.SystemSpecialists.Where(ss => ss.UserId == user.UserId).Select(ss => ss.SystemId).ToList();
    }

    private bool IsSystemOwner(User user) => HasRole(user, "SystemManager");
    private bool IsGeneralManager(User user) => HasRole(user, "GeneralManager");
    private bool HasRole(User user, string roleName) => _context.UserRoles.Where(ur => ur.UserId == user.UserId)
        .Join(_context.Roles, ur => ur.RoleId, r => r.RoleId, (ur, r) => r.RoleName).Contains(roleName);
    private bool CanAccessDepartment(User user, int deptId) => user.SubDeptId == deptId ||
        _context.UserDepartmentAccesses.Any(uda => uda.UserId == user.UserId && uda.SubDeptId == deptId && uda.IsActive);
    private bool IsUnderGeneralDept(User user, int subDeptId)
    {
        var userGd = _context.SubDepartments.Where(sd => sd.SubDeptId == user.SubDeptId).Select(sd => sd.GeneralDeptId).FirstOrDefault();
        var targetGd = _context.SubDepartments.Where(sd => sd.SubDeptId == subDeptId).Select(sd => sd.GeneralDeptId).FirstOrDefault();
        return userGd == targetGd;
    }
}
