using Microsoft.Extensions.Options;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.API.Services;

public class FileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _settings;
    public FileStorageService(IOptions<FileStorageSettings> settings) => _settings = settings.Value;

    public async Task<string> SaveFileAsync(string ticketNumber, IFormFile file)
    {
        var ticketFolder = Path.Combine(_settings.BasePath, ticketNumber);
        Directory.CreateDirectory(ticketFolder);
        var uniqueFileName = $"{Guid.NewGuid():N}_{file.FileName}";
        var fullPath = Path.Combine(ticketFolder, uniqueFileName);
        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);
        return fullPath;
    }

    public async Task DeleteFileAsync(string filePath)
    {
        if (File.Exists(filePath)) File.Delete(filePath);
    }
}

public class FileStorageSettings
{
    public string BasePath { get; set; } = @"C:\TicketingSystem\Attachments";
    public int MaxFileSizeMB { get; set; } = 10;
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
}

public class PasswordPolicy
{
    public int MinLength { get; set; } = 12;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialChar { get; set; } = true;
    public int MaxAgeDays { get; set; } = 90;
    public int HistoryCount { get; set; } = 5;
    public int LockoutAfterAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 30;
}
