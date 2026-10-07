using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Core.DTOs;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "اسم المستخدم وكلمة المرور مطلوبان" });

        var result = await _authService.LoginAsync(request);

        if (result.User == null)
            return Unauthorized(new { message = "بيانات الدخول غير صحيحة أو الحساب غير مفعل" });

        if (result.RequiresMfa)
            return Ok(new { requiresMfa = true, message = "أدخل رمز التحقق الثنائي" });

        return Ok(new { token = result.Token, user = result.User });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        await _authService.LogoutAsync(userId);
        return Ok(new { message = "تم تسجيل الخروج بنجاح" });
    }
}
