namespace TicketingSystem.Core.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(string ticketNumber, IFormFile file);
    Task DeleteFileAsync(string filePath);
}
