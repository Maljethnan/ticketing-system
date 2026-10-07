using System.Collections.Generic;
using System.Threading.Tasks;

namespace TicketingSystem.Core.Services;

public interface IFileStorageService
{
    Task<string> SaveAsync(byte[] data, string fileName, string folder);
    byte[] ReadAsync(string filePath);
    void DeleteAsync(string filePath);
    List<string> ListFilesAsync(string folder);
}
