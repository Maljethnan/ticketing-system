using TicketingSystem.Core.DTOs;

namespace TicketingSystem.Web.Services;

public class AuthService
{
    private UserDto? _currentUser;
    private string? _token;

    public UserDto? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null;

    public Task SetCurrentUserAsync(UserDto user)
    {
        _currentUser = user;
        return Task.CompletedTask;
    }

    public Task<string?> GetTokenAsync() => Task.FromResult(_token);

    public Task LogoutAsync()
    {
        _currentUser = null;
        _token = null;
        return Task.CompletedTask;
    }
}
