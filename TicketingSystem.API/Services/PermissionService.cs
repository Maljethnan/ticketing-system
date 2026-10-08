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

        return _context.RolePermissions.Any(rp =>
            rp.Resource == resource &&
            rp.Action == action &&
            _context.UserRoles.Any(ur => ur.UserId == user.Id && ur.RoleId == rp.RoleId));
    }

    public bool CanViewTicket(User user, Ticket ticket)
    {
        if (IsSystemOwner(user)) return true;
        if (user.Id == ticket.CreatorId) return true;
        if (ticket.AssignedTo == user.Id) return true;
        if (_context.TicketAccessGrants.Any(g => g.TicketId == ticket.TicketId && g.UserId == user.Id && g.IsActive)) return true;
        if (CanAccessDepartment(user, ticket.CreatorDeptId)) return true;
        if (IsGeneralManager(user)) return true;
        return false;
    }

    public bool CanEditTicket(User user, Ticket ticket)
    {
        if (IsSystemOwner(user)) return true;
        if (ticket.AssignedTo == user.Id) return true;
        if (user.Id == ticket.CreatorId) return true;
        return false;
    }

    public bool CanCloseTicket(User user, Ticket ticket)
    {
        if (IsSystemOwner(user)) return true;
        if (ticket.AssignedTo == user.Id) return true;
        return false;
    }

    public bool CanApproveClosure(User user, Ticket ticket)
    {
        if (IsSystemOwner(user)) return true;
        if (user.Id == ticket.CreatorId) return true;
        if (HasRole(user, "GM")) return true;
        return false;
    }

    public bool CanCommentOnTicket(User user, Ticket ticket) => CanViewTicket(user, ticket);

    public bool CanViewAllTickets(User user) => IsSystemOwner(user) || IsGeneralManager(user);

    public bool CanManageSettings(User user) => IsSystemOwner(user);

    public List<int> GetAccessibleDepartmentIds(User user)
    {
        var deptIds = new HashSet<int>();

        if (user.GeneralDeptId.HasValue)
            deptIds.Add(user.GeneralDeptId.Value);

        deptIds.UnionWith(
            _context.UserDepartmentAccesses
                .Where(uda => uda.UserId == user.Id && uda.IsActive)
                .Select(uda => uda.SubDeptId)
                .ToList());

        if (IsGeneralManager(user) && user.GeneralDeptId.HasValue)
        {
            deptIds.UnionWith(
                _context.GeneralDepartments
                    .Where(d => d.IsActive)
                    .Select(d => d.GeneralDeptId)
                    .ToList());
        }

        return deptIds.ToList();
    }

    public List<int> GetAccessibleSystemIds(User user)
    {
        if (IsSystemOwner(user) || IsGeneralManager(user))
            return _context.Systems.Where(s => s.IsActive).Select(s => s.SystemId).ToList();

        return _context.SystemSpecialists
            .Where(ss => ss.UserId == user.Id)
            .Select(ss => ss.SystemId)
            .ToList();
    }

    private bool IsSystemOwner(User user)
    {
        return _context.UserRoles.Any(ur =>
            ur.UserId == user.Id &&
            _context.Roles.Any(r => r.RoleId == ur.RoleId && MatchesRoleName(r.RoleName, "Owner")));
    }

    private bool IsGeneralManager(User user)
    {
        return _context.UserRoles.Any(ur =>
            ur.UserId == user.Id &&
            _context.Roles.Any(r => r.RoleId == ur.RoleId && MatchesRoleName(r.RoleName, "GeneralManager", "General Manager")));
    }

    private bool HasRole(User user, string roleKeyword)
    {
        return _context.UserRoles.Any(ur =>
            ur.UserId == user.Id &&
            _context.Roles.Any(r => r.RoleId == ur.RoleId && MatchesRoleName(r.RoleName, roleKeyword)));
    }

    private static bool MatchesRoleName(string roleName, params string[] aliases)
    {
        var normalizedRoleName = roleName.Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase);

        foreach (var alias in aliases)
        {
            var normalizedAlias = alias.Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase);
            if (normalizedRoleName.Contains(normalizedAlias, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private bool CanAccessDepartment(User user, int deptId)
    {
        if (user.GeneralDeptId == deptId) return true;
        return _context.UserDepartmentAccesses.Any(uda =>
            uda.UserId == user.Id && uda.SubDeptId == deptId && uda.IsActive);
    }
}
