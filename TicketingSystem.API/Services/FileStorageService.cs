using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TicketingSystem.Core.Services;

namespace TicketingSystem.API.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public FileStorageService(string rootPath)
    {
        _rootPath = Path.GetFullPath(rootPath);
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(byte[] data, string fileName, string folder)
    {
        var dirPath = Path.Combine(_rootPath, folder);
        Directory.CreateDirectory(dirPath);
        var filePath = Path.Combine(dirPath, Guid.NewGuid() + "_" + fileName);
        await File.WriteAllBytesAsync(filePath, data);
        return filePath;
    }

    public byte[] ReadAsync(string filePath)
    {
        return File.ReadAllBytes(filePath);
    }

    public void DeleteAsync(string filePath)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    public List<string> ListFilesAsync(string folder)
    {
        var dirPath = Path.Combine(_rootPath, folder);
        if (!Directory.Exists(dirPath))
            return new List<string>();
        return Directory.GetFiles(dirPath).ToList();
    }
}
