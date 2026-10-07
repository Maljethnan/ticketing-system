namespace TicketingSystem.API.Configuration;

public class FileStorageSettings
{
    public string RootPath { get; set; } = "./uploads";
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10MB
    public string[] AllowedExtensions { get; set; } = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".docx", ".xlsx", ".zip" };
}

public class PasswordPolicy
{
    public int MinLength { get; set; } = 8;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialChar { get; set; } = true;
}
