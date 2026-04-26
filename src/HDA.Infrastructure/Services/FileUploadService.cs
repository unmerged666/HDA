using Microsoft.AspNetCore.Components.Forms;

namespace HDA.Infrastructure.Services;

public interface IFileUploadService
{
    Task<string?> UploadAvatarAsync(IBrowserFile file, string folder = "avatars");
    Task<string?> UploadImageAsync(IBrowserFile file, string folder = "images");
    void DeleteFile(string relativePath);
}

public class FileUploadService : IFileUploadService
{
    private readonly string _webRootPath;
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    public FileUploadService(string webRootPath) => _webRootPath = webRootPath;

    public async Task<string?> UploadAvatarAsync(IBrowserFile file, string folder = "avatars")
    {
        if (!IsValidImage(file)) return null;

        var upload = Path.Combine(_webRootPath, "uploads", folder);
        Directory.CreateDirectory(upload);

        var ext = Path.GetExtension(file.Name).ToLowerInvariant();
        var filename = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(upload, filename);

        await using var fs = new FileStream(fullPath, FileMode.Create);
        await file.OpenReadStream(MaxFileSize).CopyToAsync(fs);

        return $"/uploads/{folder}/{filename}";
    }

    public Task<string?> UploadImageAsync(IBrowserFile file, string folder = "images") =>
        UploadAvatarAsync(file, folder);

    public void DeleteFile(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return;
        var fullPath = Path.Combine(_webRootPath, relativePath.TrimStart('/'));
        if (File.Exists(fullPath)) File.Delete(fullPath);
    }

    private static bool IsValidImage(IBrowserFile file)
    {
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        return allowedTypes.Contains(file.ContentType) && file.Size <= 5 * 1024 * 1024;
    }
}
