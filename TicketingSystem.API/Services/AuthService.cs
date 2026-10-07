using System.DirectoryServices.Protocols;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.DTOs;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Data.Context;
using TicketingSystem.API.Dtos;

namespace TicketingSystem.API.Services;

/// <summary>
/// خدمة المصادقة — تدعم مسارين:
/// 1. LDAP: التحقق من المستخدم عبر دليل Active Directory
/// 2. Local: مصادقة المستخدمين المسجلين محليًا (إنشاؤهم حصريًا بواسطة المدير العام)
/// </summary>
public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    // إعدادات LDAP من متغيرات البيئة
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
    // المسار الأول: مصادقة LDAP (للمستخدمين الموجودين في AD)
    // ================================================================

    public async Task<AuthResponseDto?> AuthenticateAsync(string username, string password)
    {
        try
        {
            bool isValid = await ValidateLdapCredentialsAsync(request.Username, request.Password);

            if (!isValid)
                return new AuthResponseDto { Success = false, Message = "اسم المستخدم أو كلمة المرور غير صحيحة" };

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Username || u.Username == request.Username);

            if (user == null)
            {
                user = await CreateLocalUserFromLdapAsync(request.Username);
            }

            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Success = true,
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    RoleCode = user.RoleCode,
                    DepartmentId = user.DepartmentId
                },
                Message = "تم تسجيل الدخول بنجاح"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطأ في المصادقة عبر LDAP للمستخدم: {Username}", request.Username);
            return new AuthResponseDto { Success = false, Message = "حدث خطأ أثناء الاتصال بالدليل النشط" };
        }
    }

    /// <summary>
    /// التحقق من صحة بيانات الاعتماد عبر بروتوكول LDAP
    /// </summary>
    private async Task<bool> ValidateLdapCredentialsAsync(string username, string password)
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
                return false;

            // محاولة الربط باسم المستخدم وكلمة المرور (Authenticated Bind)
            var bindConnection = new LdapConnection(new LdapDirectoryIdentifier(LdapServer, LdapPort));
            bindConnection.Credential = new NetworkCredential(username, password);
            bindConnection.AuthType = AuthType.Basic;
            bindConnection.SessionOptions.ProtocolVersion = 3;
            bindConnection.Bind();

            return true;
        }
        catch (LdapException ex)
        {
            _logger.LogWarning(ex, "فشل التحقق من LDAP للمستخدم: {Username}", username);
            return false;
        }
    }

    /// <summary>
    /// إنشاء مستخدم محلي عند أول دخول عبر AD
    /// </summary>
    private async Task<User> CreateLocalUserFromLdapAsync(string username)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Code == "EMP");
        if (role == null)
            throw new InvalidOperationException("دور Employee غير موجود — تأكد من تهيئة الأدوار");

        var user = new User
        {
            Username = username,
            Email = $"{username}@company.local",
            FullName = username,
            PasswordHash = null,
            RoleId = role.Id,
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
    // المسار الثاني: إنشاء مستخدم جديد (حصريًا للمدير العام GM)
    // ================================================================

    public async Task<AuthResponseDto?> CreateLocalUserAsync(int createdByUserId, string username, string email, string? fullName, string password, int roleId, int? departmentId)
    {
        try
        {
            // 1. التحقق من عدم تكرار البريد أو اسم المستخدم
            var existing = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == request.Email || u.Username == request.Username);

            if (existing != null)
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "البريد الإلكتروني أو اسم المستخدم مستخدم بالفعل"
                };

            // 2. تشفير كلمة المرور الافتراضية التي اختارها المدير
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.DefaultPassword);

            // 3. تحديد الدور المطلوب
            var role = await _context.Roles.FindAsync(request.RoleId);
            if (role == null)
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "الدور المحدد غير موجود"
                };

            // 4. التحقق من وجود الإدارة (إذا تم تحديدها)
            if (request.DepartmentId.HasValue)
            {
                var dept = await _context.Departments.FindAsync(request.DepartmentId.Value);
                if (dept == null)
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "الإدارة المحددة غير موجودة"
                    };
            }

            // 5. إنشاء المستخدم بحالة "يجب تغيير كلمة المرور"
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = passwordHash,
                RoleId = role.Id,
                DepartmentId = request.DepartmentId,
                IsActive = true,
                IsAdUser = false,
                MustChangePassword = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 6. تسجيل الحدث في Audit Log
            await LogAuditActionAsync(-1, "USER_CREATED_BY_ADMIN",
                $"المدير أنشأ مستخدمًا جديدًا: {request.Username} | الدور: {role.Name}");

            _logger.LogInformation("المدير أنشأ مستخدمًا جديدًا: {Username} | الدور: {RoleName}",
                request.Username, role.Name);

            return new AuthResponseDto
            {
                Success = true,
                Message = $"تم إنشاء حساب المستخدم '{request.Username}' بنجاح — يجب عليه تغيير كلمة المرور عند أول دخول"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطأ في إنشاء المستخدم الجديد بواسطة المدير");
            return new AuthResponseDto
            {
                Success = false,
                Message = "حدث خطأ أثناء إنشاء الحساب"
            };
        }
    }

    // ================================================================
    // مصادقة محلية (للمستخدمين المسجلين بدون AD)
    // ================================================================

    AuthResponseDto? AuthenticateLocalAsync(LoginRequestDto request)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    (u.Email == request.Username || u.Username == request.Username)
                    && u.IsAdUser == false);

            if (user == null || user.PasswordHash == null)
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "المستخدم غير موجود أو يستخدم مصادقة AD"
                };

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "كلمة المرور غير صحيحة"
                };

            if (!user.IsActive)
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "حسابك معطل — تواصل مع المدير"
                };

            // ⚠️ إذا كان يجب تغيير كلمة المرور، نرفض إصدار Token عادي
            if (user.MustChangePassword)
            {
                return new AuthResponseDto
                {
                    Success = true,
                    MustChangePassword = true,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        FullName = user.FullName,
                        RoleCode = user.RoleCode,
                        DepartmentId = user.DepartmentId
                    },
                    Message = "مرحبًا بك! يرجى تغيير كلمة المرور قبل المتابعة"
                };
            }

            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Success = true,
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    RoleCode = user.RoleCode,
                    DepartmentId = user.DepartmentId
                },
                Message = "تم تسجيل الدخول بنجاح"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطأ في المصادقة المحلية للمستخدم: {Username}", request.Username);
            return new AuthResponseDto { Success = false, Message = "حدث خطأ أثناء تسجيل الدخول" };
        }
    }

    // ================================================================
    // تغيير كلمة المرور (إجباري عند أول دخول)
    // ================================================================

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "المستخدم غير موجود"
                };

            // التحقق من كلمة المرور الحالية (للتأكد من هوية المستخدم)
            if (user.PasswordHash != null && !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "كلمة المرور الحالية غير صحيحة"
                };

            // التحقق من قوة كلمة المرور الجديدة
            var validation = ValidatePassword(request.NewPassword);
            if (!validation.IsValid)
                return new AuthResponseDto
                {
                    Success = false,
                    Message = string.Join(" | ", validation.Errors)
                };

            // تحديث كلمة المرور
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.MustChangePassword = false;
            user.LastLoginAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await LogAuditActionAsync(userId, "PASSWORD_CHANGED",
                $"تغيير كلمة المرور - أول دخول: {user.Username}");

            _logger.LogInformation("تم تغيير كلمة المرور بنجاح: {Username}", user.Username);

            // إصدار Token جديد بعد التغيير
            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Success = true,
                Token = token,
                Message = "تم تغيير كلمة المرور بنجاح"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطأ في تغيير كلمة المرور للمستخدم: {UserId}", userId);
            return new AuthResponseDto
            {
                Success = false,
                Message = "حدث خطأ أثناء تغيير كلمة المرور"
            };
        }
    }

    // ================================================================
    // أدوات مشتركة
    // ================================================================

    private string GenerateJwtToken(User user)
    {
        var secretKey = _config["JwtSettings:SecretKey"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.RoleCode),
            new Claim("DepartmentId", user.DepartmentId?.ToString() ?? ""),
            new Claim("IsAdUser", user.IsAdUser.ToString())
        };

        var expiryMinutes = int.Parse(_config["JwtSettings:ExpiryMinutes"] ?? "60");
        var token = new JwtSecurityToken(
            issuer: "TicketingSystem",
            audience: "TicketingSystem",
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
            Timestamp = DateTime.UtcNow
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

// ================================================================
// DTOs المساعدة
// ================================================================

public record CreateUserRequestDto(
    string Username,
    string Email,
    string FullName,
    string DefaultPassword,
    int RoleId,
    int? DepartmentId
);

public record ChangePasswordRequestDto(
    string CurrentPassword,
    string NewPassword
);

public record PasswordValidationResult
{
    public bool IsValid { get; init; }
    public List<string> Errors { get; init; } = new();
}
