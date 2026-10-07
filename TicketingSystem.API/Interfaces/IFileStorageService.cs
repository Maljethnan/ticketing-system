using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
namespace TicketingSystem.API.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(string ticketNumber, IFormFile file);
    Task DeleteFileAsync(string filePath);
}
