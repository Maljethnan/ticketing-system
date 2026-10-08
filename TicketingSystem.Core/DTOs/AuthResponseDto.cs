namespace TicketingSystem.Core.DTOs;

public class AuthResponseDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsAdUser { get; set; }
    public bool MustChangePassword { get; set; }
    public string? Token { get; set; }
}
