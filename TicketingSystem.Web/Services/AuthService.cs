using System;
using System.Threading.Tasks;
using TicketingSystem.Core.DTOs;

namespace TicketingSystem.Web.Services;

public class AuthService
{
    private UserDto? _currentUser;

    public async Task<bool> LoginAsync(string username, string password)
    {
        // TODO: Call API auth endpoint
        return true;
    }

    public async Task SetCurrentUserAsync(UserDto user)
    {
        _currentUser = user;
    }

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        return _currentUser;
    }

    public bool IsAuthenticated => _currentUser != null;

    public string? CurrentRole => _currentUser?.RoleName;

    public int? CurrentUserId => _currentUser?.UserId;

    public void Logout()
    {
        _currentUser = null;
    }
}
