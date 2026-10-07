using TicketingSystem.Core.DTOs;

namespace TicketingSystem.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> AuthenticateAsync(string username, string password);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<AuthResponseDto?> CreateLocalUserAsync(int createdByUserId, string username, string email, string? fullName, string password, int roleId, int? departmentId);
}
