using System;
using System.Threading.Tasks;
using TicketingSystem.Core.DTOs;

namespace TicketingSystem.Web.Services;

public class AuthService
{
    public event Action? AuthStateChanged;

    private UserDto? _currentUser;
    private string? _token;

    public Task<bool> LoginAsync(string username, string password)
    {
        return Task.FromResult(_currentUser != null && !string.IsNullOrWhiteSpace(_token));
    }

    public Task SetCurrentUserAsync(UserDto? user, string? token = null)
    {
        _currentUser = user;
        if (!string.IsNullOrWhiteSpace(token))
            _token = token;

        AuthStateChanged?.Invoke();
        return Task.CompletedTask;
    }

    public Task<UserDto?> GetCurrentUserAsync()
    {
        return Task.FromResult(_currentUser);
    }

    public string? CurrentToken => _token;

    public bool IsAuthenticated => _currentUser != null;

    public string? CurrentRole => _currentUser?.RoleName;

    public int? CurrentUserId => _currentUser?.UserId;

    public void Logout()
    {
        _currentUser = null;
        _token = null;
        AuthStateChanged?.Invoke();
    }
}
