using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using TicketingSystem.Web.Services;

namespace TicketingSystem.Web.Services;

public class BlazorAuthStateProvider : AuthenticationStateProvider
{
    private readonly AuthService _authService;

    public BlazorAuthStateProvider(AuthService authService)
    {
        _authService = authService;
        _authService.AuthStateChanged += OnAuthStateChanged;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = await _authService.GetCurrentUserAsync();
        var identity = user == null
            ? new ClaimsIdentity()
            : new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, user.RoleName)
            }, "tickets-auth");

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    private void OnAuthStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
