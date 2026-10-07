using TicketingSystem.Core.DTOs;

namespace TicketingSystem.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(LoginRequest request);
    Task<bool> LogoutAsync(int userId);
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task<UserDto> CreateUserAsync(CreateUserRequest request);
    Task UpdateUserRoleAsync(int userId, int roleId);
    Task LockUserAsync(int userId);
    Task UnlockUserAsync(int userId);
}
