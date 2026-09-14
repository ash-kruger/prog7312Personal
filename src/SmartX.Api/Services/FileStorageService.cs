namespace SmartX.Api.Services;

using System.Text;

/// <summary>
/// Saves device configuration files, deployment photos, or hardware log
/// files uploaded through the multipart file endpoint, namespaced by
/// device MAC address so each sensor's attachments stay together on disk.
/// </summary>
public sealed class FileStorageService
{
    private readonly string _rootPath;

    public FileStorageService(IWebHostEnvironment env)
    {
        _rootPath = Path.Combine(env.ContentRootPath, "UploadedFiles");
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(string deviceMac, IFormFile file)
    {
        var safeMac = SanitiseForPath(deviceMac);
        var deviceFolder = Path.Combine(_rootPath, safeMac);
        Directory.CreateDirectory(deviceFolder);

        var safeFileName = Path.GetFileName(file.FileName);
        var fileName = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmss}_{safeFileName}";
        var fullPath = Path.Combine(deviceFolder, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Path.Combine(safeMac, fileName).Replace('\\', '/');
    }

    private static string SanitiseForPath(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);

        foreach (var c in value)
        {
            builder.Append(c == ':' || Array.IndexOf(invalid, c) >= 0 ? '-' : c);
        }

        return builder.ToString();
    }
}
