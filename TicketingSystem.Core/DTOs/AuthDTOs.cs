namespace TicketingSystem.Core.DTOs;

public record AuthResult(string Token, UserDto User, bool RequiresMfa);
public record LoginRequest(string UserName, string Password, string? TfaCode);
public record AuthLoginResponse(bool Success, bool RequiresMfa, string? Token, UserDto? User);
public record CreateUserRequest(string UserName, string Email, string Password, int SubDeptId, List<int> RoleIds);
public record UserDto(int UserId, string UserName, string? Email, string RoleName, string DepartmentName, bool IsActive);
