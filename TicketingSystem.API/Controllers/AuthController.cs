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

        var result = await _authService.AuthenticateAsync(request.UserName, request.Password);

        if (result == null)
            return Unauthorized(new { message = "بيانات الدخول غير صحيحة أو الحساب غير مفعل" });

        var user = new UserDto(
            result.UserId,
            result.Username,
            result.Email,
            result.RoleName,
            string.Empty,
            true);

        return Ok(new AuthLoginResponse(
            true,
            false,
            result.Token,
            user));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        var success = await _authService.ChangePasswordAsync(
            userId, request.CurrentPassword, request.NewPassword);

        if (!success)
            return BadRequest(new { message = "فشل تغيير كلمة المرور — تأكد من كلمة المرور الحالية وقوة كلمة المرور الجديدة" });

        return Ok(new { message = "تم تغيير كلمة المرور بنجاح" });
    }

    [Authorize]
    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var isPrivileged = User.IsInRole("SystemManager") || User.IsInRole("GeneralManager");
        if (!isPrivileged)
            return Forbid();

        var adminId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        var result = await _authService.CreateLocalUserAsync(
            adminId,
            request.UserName,
            request.Email,
            null,
            request.Password,
            request.RoleIds.FirstOrDefault(),
            request.SubDeptId);

        if (result == null)
            return BadRequest(new { message = "فشل إنشاء المستخدم — قد يكون الاسم أو البريد مستخدمًا مسبقًا" });

        return Ok(new
        {
            message = "تم إنشاء المستخدم بنجاح — يجب عليه تغيير كلمة المرور عند أول دخول",
            userId = result.UserId,
            username = result.Username
        });
    }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
}
