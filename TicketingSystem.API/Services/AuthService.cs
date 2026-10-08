using System.DirectoryServices.Protocols;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.DTOs;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Data.Context;

namespace TicketingSystem.API.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    private string LdapServer => _config["LdapSettings:Server"] ?? "ad.company.local";
    private int LdapPort => int.Parse(_config["LdapSettings:Port"] ?? "389");
    private string LdapBaseDn => _config["LdapSettings:BaseDn"] ?? "DC=company,DC=local";
    private string LdapBindUser => _config["LdapSettings:BindUser"] ?? "";
    private string LdapBindPassword => _config["LdapSettings:BindPassword"] ?? "";

    public AuthService(AppDbContext context, IConfiguration config, ILogger<AuthService> logger)
    {
        _context = context;
        _config = config;
        _logger = logger;
    }

    // ================================================================
    // مصادقة المستخدم عبر LDAP
    // ================================================================

    public async Task<AuthResponseDto?> AuthenticateAsync(string username, string password)
    {
        try
        {
            var normalizedUserName = username.Trim();
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == normalizedUserName || u.Email == normalizedUserName);

            if (user != null && !string.IsNullOrWhiteSpace(user.PasswordHash) && VerifyStoredPassword(password, user.PasswordHash))
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new AuthResponseDto
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    RoleName = user.Role?.RoleName ?? "",
                    IsAdUser = user.IsAdUser,
                    MustChangePassword = user.MustChangePassword,
                    Token = GenerateJwtToken(user)
                };
            }

            var isValidViaLdap = await ValidateLdapCredentialsAsync(normalizedUserName, password);
            if (!isValidViaLdap)
                return null;

            user ??= await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == normalizedUserName || u.Email == normalizedUserName);

            if (user == null)
            {
                user = await CreateLocalUserFromLdapAsync(normalizedUserName);
                await _context.Entry(user).Reference(u => u.Role).LoadAsync();
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                RoleName = user.Role?.RoleName ?? "",
                IsAdUser = user.IsAdUser,
                MustChangePassword = user.MustChangePassword,
                Token = GenerateJwtToken(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطأ في المصادقة للمستخدم: {Username}", username);
            return null;
        }
    }

    /// <summary>
    /// التحقق من صحة بيانات الاعتماد عبر بروتوكول LDAP
    /// </summary>
    private Task<bool> ValidateLdapCredentialsAsync(string username, string password)
    {
        try
        {
            using var connection = new LdapConnection(
                new LdapDirectoryIdentifier(LdapServer, LdapPort));

            if (!string.IsNullOrEmpty(LdapBindUser))
            {
                connection.Credential = new NetworkCredential(LdapBindUser, LdapBindPassword);
                connection.AuthType = AuthType.Basic;
            }

            connection.SessionOptions.ProtocolVersion = 3;
            connection.Bind();

            var searchFilter = $"(&(objectClass=user)(sAMAccountName={username}))";
            var searchRequest = new SearchRequest(
                LdapBaseDn,
                searchFilter,
                System.DirectoryServices.Protocols.SearchScope.Subtree,
                new[] { "distinguishedName", "cn", "mail" });

            SearchResponse searchResponse = (SearchResponse)connection.SendRequest(searchRequest);

            if (searchResponse.Entries.Count == 0)
                return Task.FromResult(false);

            // محاولة الربط باسم المستخدم وكلمة المرور (Authenticated Bind)
            var bindConnection = new LdapConnection(new LdapDirectoryIdentifier(LdapServer, LdapPort));
            bindConnection.Credential = new NetworkCredential(username, password);
            bindConnection.AuthType = AuthType.Basic;
            bindConnection.SessionOptions.ProtocolVersion = 3;
            bindConnection.Bind();

            return Task.FromResult(true);
        }
        catch (LdapException ex)
        {
            _logger.LogWarning(ex, "فشل التحقق من LDAP للمستخدم: {Username}", username);
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// إنشاء مستخدم محلي عند أول دخول عبر AD
    /// </summary>
    private async Task<User> CreateLocalUserFromLdapAsync(string username)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName.Contains("Employee", StringComparison.OrdinalIgnoreCase));
        if (role == null)
            throw new InvalidOperationException("دور Employee غير موجود — تأكد من تهيئة الأدوار");

        var user = new User
        {
            Username = username,
            Email = $"{username}@company.local",
            FullName = username,
            PasswordHash = null,
            RoleId = role.RoleId,
            IsActive = true,
            IsAdUser = true,
            MustChangePassword = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("تم إنشاء مستخدم محلي تلقائيًا من AD: {Username}", username);
        return user;
    }

    // ================================================================
    // إنشاء مستخدم جديد بواسطة المدير العام
    // ================================================================

    public async Task<AuthResponseDto?> CreateLocalUserAsync(
        int createdByUserId, string username, string email, string? fullName,
        string password, int roleId, int? departmentId)
    {
        try
        {
            var existing = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email || u.Username == username);

            if (existing != null)
                return null;

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var role = await _context.Roles.FindAsync(roleId);
            if (role == null)
                return null;

            if (departmentId.HasValue)
            {
                var dept = await _context.GeneralDepartments.FindAsync(departmentId.Value);
                if (dept == null)
                    return null;
            }

            var user = new User
            {
                Username = username,
                Email = email,
                FullName = fullName,
                PasswordHash = passwordHash,
                RoleId = roleId,
                GeneralDeptId = departmentId,
                IsActive = true,
                IsAdUser = false,
                MustChangePassword = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await LogAuditActionAsync(createdByUserId, "USER_CREATED_BY_ADMIN",
                $"المدير أنشأ مستخدمًا جديدًا: {username} | الدور: {role.RoleName}");

            _logger.LogInformation("المدير أنشأ مستخدمًا جديدًا: {Username} | الدور: {RoleName}",
                username, role.RoleName);

            return new AuthResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                RoleName = role.RoleName,
                IsAdUser = false,
                MustChangePassword = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطأ في إنشاء المستخدم الجديد بواسطة المدير");
            return null;
        }
    }

    // ================================================================
    // تغيير كلمة المرور
    // ================================================================

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            if (user.PasswordHash != null && !VerifyStoredPassword(currentPassword, user.PasswordHash))
                return false;

            var validation = ValidatePassword(newPassword);
            if (!validation.IsValid)
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.MustChangePassword = false;
            user.LastLoginAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await LogAuditActionAsync(userId, "PASSWORD_CHANGED",
                $"تغيير كلمة المرور - أول دخول: {user.Username}");

            _logger.LogInformation("تم تغيير كلمة المرور بنجاح: {Username}", user.Username);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطأ في تغيير كلمة المرور للمستخدم: {UserId}", userId);
            return false;
        }
    }

    // ================================================================
    // أدوات مشتركة
    // ================================================================

    private static bool VerifyStoredPassword(string password, string? storedHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
            return false;

        try
        {
            if (storedHash.StartsWith("$2") || storedHash.StartsWith("$2a") || storedHash.StartsWith("$2b") || storedHash.StartsWith("$2y"))
                return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
        catch
        {
            // Ignore invalid BCrypt payloads and fall back to PBKDF2 parsing.
        }

        var parts = storedHash.Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
            return false;

        if (!int.TryParse(parts[2], out var iterations) || iterations <= 0)
            return false;

        try
        {
            var salt = Convert.FromBase64String(parts[0]);
            var expectedHash = Convert.FromBase64String(parts[1]);
            var generatedHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);
            return CryptographicOperations.FixedTimeEquals(generatedHash, expectedHash);
        }
        catch
        {
            return false;
        }
    }

    private string GenerateJwtToken(User user)
    {
        var secretKey = _config["Jwt:SecretKey"]
            ?? _config["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JWT Secret Key not configured");
        var issuer = _config["Jwt:Issuer"] ?? _config["JwtSettings:Issuer"] ?? "TicketingSystem";
        var audience = _config["Jwt:Audience"] ?? _config["JwtSettings:Audience"] ?? "TicketingSystem";
        var expiryMinutes = _config.GetValue<int>("Jwt:ExpirationMinutes", _config.GetValue<int>("JwtSettings:ExpiryMinutes", 60));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role?.RoleName ?? ""),
            new Claim("GeneralDeptId", user.GeneralDeptId?.ToString() ?? ""),
            new Claim("IsAdUser", user.IsAdUser.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task LogAuditActionAsync(int userId, string action, string details)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = action,
            Details = details,
            IpAddress = "",
            LoggedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    private PasswordValidationResult ValidatePassword(string password)
    {
        var errors = new List<string>();

        if (password.Length < 8)
            errors.Add("كلمة المرور يجب أن تكون 8 أحرف على الأقل");
        if (!password.Any(char.IsUpper))
            errors.Add("يجب أن تحتوي على حرف كبير واحد على الأقل");
        if (!password.Any(char.IsLower))
            errors.Add("يجب أن تحتوي على حرف صغير واحد على الأقل");
        if (!password.Any(char.IsDigit))
            errors.Add("يجب أن تحتوي على رقم واحد على الأقل");
        if (!password.Any(c => !char.IsLetterOrDigit(c)))
            errors.Add("يجب أن تحتوي على رمز خاص واحد على الأقل (@#$%...)");

        return new PasswordValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}

public record PasswordValidationResult
{
    public bool IsValid { get; init; }
    public List<string> Errors { get; init; } = new();
}
