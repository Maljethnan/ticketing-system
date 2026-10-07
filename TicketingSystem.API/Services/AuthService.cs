using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TicketingSystem.Core.DTOs;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Data.Context;
using TicketingSystem.Data.Repositories;

namespace TicketingSystem.API.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly IAuditService _auditService;
    private readonly IUserRepository _userRepository;

    public AuthService(AppDbContext context, IConfiguration config, IAuditService auditService, IUserRepository userRepository)
    {
        _context = context;
        _config = config;
        _auditService = auditService;
        _userRepository = userRepository;
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.UserName);

        if (user == null || !user.IsActive)
            return new AuthResult(string.Empty, null!, false);

        // Check lockout
        if (user.IsLocked && user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
            return new AuthResult(string.Empty, null!, false);

        bool passwordValid;
        if (user.UserName == "admin")
        {
            passwordValid = VerifyPassword(request.Password, user.PasswordHash);
        }
        else
        {
            passwordValid = await ValidateAgainstActiveDirectory(user.UserName, request.Password);
        }

        if (!passwordValid)
        {
            await IncrementFailedAttempts(user);
            return new AuthResult(string.Empty, null!, false);
        }

        user.FailedLoginAttempts = 0;
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(user.UserId, "Login", "User", user.UserId, "Successful login");

        if (user.TfaEnabled)
        {
            if (string.IsNullOrEmpty(request.TfaCode))
                return new AuthResult(string.Empty, MapToDto(user), true);
            if (!VerifyTfaCode(user.TfaSecret!, request.TfaCode))
                return new AuthResult(string.Empty, null!, false);
        }

        var token = GenerateJwtToken(user);
        return new AuthResult(token, MapToDto(user), false);
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new("Email", user.Email ?? ""),
            new("DeptId", user.SubDeptId?.ToString() ?? "")
        };

        var roles = _context.UserRoles
            .Where(ur => ur.UserId == user.UserId)
            .Join(_context.Roles, ur => ur.RoleId, r => r.RoleId, (ur, r) => r.RoleName)
            .ToList();

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpirationMinutes"] ?? "480")),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<bool> ValidateAgainstActiveDirectory(string username, string password)
    {
        try
        {
            var domain = Environment.GetEnvironmentVariable("AD_DOMAIN") ?? "NAZHA.LOCAL";
            using var ctx = new System.DirectoryServices.AccountManagement.PrincipalContext(
                System.DirectoryServices.AccountManagement.ContextType.Domain, domain, username, password);
            return ctx.ValidateCredentials(username, password);
        }
        catch { return false; }
    }

    private bool VerifyPassword(string password, string hash)
    {
        var parts = hash.Split(':');
        var salt = Convert.FromBase64String(parts[0]);
        var storedHash = Convert.FromBase64String(parts[1]);
        var iterations = int.Parse(parts[2]);
        var computedHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
    }

    private async Task IncrementFailedAttempts(User user)
    {
        user.FailedLoginAttempts++;
        if (user.FailedLoginAttempts >= 5)
        {
            user.IsLocked = true;
            user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
        }
        await _context.SaveChangesAsync();
    }

    private UserDto MapToDto(User user)
    {
        var roleName = _context.UserRoles
            .Where(ur => ur.UserId == user.UserId)
            .Join(_context.Roles, ur => ur.RoleId, r => r.RoleId, (ur, r) => r.Description)
            .FirstOrDefault() ?? "";
        var deptName = user.SubDepartment?.DeptName ?? "";
        return new UserDto(user.UserId, user.UserName, user.Email, roleName, deptName, user.IsActive);
    }

    public async Task<bool> LogoutAsync(int userId)
    {
        await _auditService.LogAsync(userId, "Logout", "User", userId, "User logged out");
        return true;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _context.Users.Include(u => u.SubDepartment).Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ToListAsync();
        return users.Select(MapToDto).ToList();
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _context.Users.Include(u => u.SubDepartment).Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        var user = new User
        {
            UserName = request.UserName, Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            SubDeptId = request.SubDeptId, CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        foreach (var roleId in request.RoleIds)
            _context.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = roleId, AssignedBy = user.UserId });
        await _context.SaveChangesAsync();
        return MapToDto(user);
    }

    private string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}:100000";
    }

    public async Task UpdateUserRoleAsync(int userId, int roleId) { /* Implementation */ }
    public async Task LockUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null) { user.IsLocked = true; user.IsActive = false; await _context.SaveChangesAsync(); }
    }
    public async Task UnlockUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null) { user.IsLocked = false; user.IsActive = true; user.FailedLoginAttempts = 0; user.LockedUntil = null; await _context.SaveChangesAsync(); }
    }

    private bool VerifyTfaCode(string secret, string code) => true; // TOTP implementation placeholder
}
