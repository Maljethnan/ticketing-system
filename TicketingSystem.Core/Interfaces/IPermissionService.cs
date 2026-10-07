using TicketingSystem.Core.Entities;

namespace TicketingSystem.Core.Interfaces;

public interface IPermissionService
{
    bool HasPermission(User user, string resource, string action);
    bool CanViewTicket(User user, Ticket ticket);
    bool CanEditTicket(User user, Ticket ticket);
    bool CanCloseTicket(User user, Ticket ticket);
    bool CanApproveClosure(User user, Ticket ticket);
    bool CanCommentOnTicket(User user, Ticket ticket);
    bool CanViewAllTickets(User user);
    bool CanManageSettings(User user);
    List<int> GetAccessibleDepartmentIds(User user);
    List<int> GetAccessibleSystemIds(User user);
}
