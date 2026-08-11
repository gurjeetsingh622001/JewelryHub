using JewelryHub.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace JewelryHub.Infrastructure.Storage;

/// <summary>
/// Saves uploaded files under the API's wwwroot/uploads, served back out by
/// app.UseStaticFiles() in Program.cs. Fine for this stage of the project —
/// a real deployment (multiple API instances, ephemeral containers) would
/// swap this for cloud blob storage behind the same IFileStorageService
/// interface without touching any handler that calls it.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveAsync(byte[] content, string originalFileName, string folder, CancellationToken cancellationToken = default)
    {
        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var folderPath = Path.Combine(webRoot, "uploads", folder);
        Directory.CreateDirectory(folderPath);

        // Guid-named on disk regardless of the original file name — avoids
        // collisions and path-traversal/invalid-character concerns from a
        // user-supplied name; the original name is still returned in the
        // DTO for display purposes only.
        var storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
        var filePath = Path.Combine(folderPath, storedFileName);
        await File.WriteAllBytesAsync(filePath, content, cancellationToken);

        return $"/uploads/{folder}/{storedFileName}";
    }
}
