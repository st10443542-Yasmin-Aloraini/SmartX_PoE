using SmartX.Shared.Domain;

namespace SmartX.Api.Services;

/// <summary>
/// Saves device configuration files, deployment photos, or hardware logs uploaded via the
/// API-based multipart file uploader, one sub-folder per sensor MAC address.
/// </summary>
public sealed class FileStorageService
{
    private readonly string _rootPath;

    public FileStorageService(IWebHostEnvironment env)
    {
        _rootPath = Path.Combine(env.ContentRootPath, "wwwroot", "uploads");
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<SensorAttachment> SaveAsync(string mac, IFormFile file, CancellationToken ct = default)
    {
        var macFolder = Path.Combine(_rootPath, SanitiseForPath(mac));
        Directory.CreateDirectory(macFolder);

        var safeName = $"{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}_{Path.GetFileName(file.FileName)}";
        var fullPath = Path.Combine(macFolder, safeName);

        await using (var stream = File.Create(fullPath))
        {
            await file.CopyToAsync(stream, ct);
        }

        return new SensorAttachment
        {
            FileName = file.FileName,
            ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            SizeBytes = file.Length,
            StoragePath = $"/uploads/{SanitiseForPath(mac)}/{safeName}"
        };
    }

    private static string SanitiseForPath(string mac) =>
        string.Concat(mac.Where(c => char.IsLetterOrDigit(c) || c is '-' or '_'));
}
