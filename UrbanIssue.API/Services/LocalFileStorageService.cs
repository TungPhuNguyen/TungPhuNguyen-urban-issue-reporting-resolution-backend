using UrbanIssue.Application.Common.Interfaces.Storage;
using UrbanIssue.Application.Common.Models;

namespace UrbanIssue.API.Services;

public sealed class LocalFileStorageService
    : IFileStorageService
{
    private static readonly HashSet<string>
        AllowedExtensions =
        new(
            StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

    private readonly IWebHostEnvironment
        _environment;

    public LocalFileStorageService(
        IWebHostEnvironment environment)
    {
        _environment =
            environment;
    }

    public async Task<StoredFile> SaveAsync(
        string folder,
        UploadFile file,
        CancellationToken cancellationToken)
    {
        var extension =
            Path.GetExtension(
                file.FileName);

        if (string.IsNullOrWhiteSpace(extension)
            || !AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                "Định dạng ảnh không được hỗ trợ.");
        }

        await using var validatedContent = new MemoryStream();
        await file.Content.CopyToAsync(
            validatedContent,
            cancellationToken);

        if (!HasExpectedImageSignature(
                validatedContent.GetBuffer().AsSpan(
                    0,
                    checked((int)validatedContent.Length)),
                extension))
        {
            throw new InvalidOperationException(
                "Nội dung file không khớp với định dạng ảnh JPG, PNG hoặc WEBP.");
        }

        validatedContent.Position = 0;

        var webRootPath =
            GetWebRootPath();

        var normalizedFolder =
            folder
                .Trim('/')
                .Replace(
                    '/',
                    Path.DirectorySeparatorChar);

        var physicalFolder =
            Path.Combine(
                webRootPath,
                normalizedFolder);

        Directory.CreateDirectory(
            physicalFolder);

        var generatedFileName =
            $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

        var physicalFilePath =
            Path.Combine(
                physicalFolder,
                generatedFileName);

        await using var outputStream =
            new FileStream(
                physicalFilePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

        await validatedContent.CopyToAsync(
            outputStream,
            cancellationToken);

        var storageKey =
            $"{folder.Trim('/')}/{generatedFileName}";

        var publicUrl =
            "/" + storageKey.Replace('\\', '/');

        return new StoredFile(
            StorageKey: storageKey,
            PublicUrl: publicUrl);
    }

    public Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken)
    {
        var webRootPath =
            Path.GetFullPath(
                GetWebRootPath());

        var physicalPath =
            Path.GetFullPath(
                Path.Combine(
                    webRootPath,
                    storageKey.Replace(
                        '/',
                        Path.DirectorySeparatorChar)));

        if (!physicalPath.StartsWith(
                webRootPath,
                StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }

        return Task.CompletedTask;
    }

    private string GetWebRootPath()
    {
        var webRootPath =
            _environment.WebRootPath;

        if (!string.IsNullOrWhiteSpace(webRootPath))
        {
            return webRootPath;
        }

        webRootPath =
            Path.Combine(
                _environment.ContentRootPath,
                "wwwroot");

        Directory.CreateDirectory(
            webRootPath);

        return webRootPath;
    }

    private static bool HasExpectedImageSignature(
        ReadOnlySpan<byte> content,
        string extension)
    {
        if (extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
        {
            return content.Length >= 3
                && content[0] == 0xFF
                && content[1] == 0xD8
                && content[2] == 0xFF;
        }

        if (extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
        {
            ReadOnlySpan<byte> png =
                [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
            return content.StartsWith(png);
        }

        return content.Length >= 12
            && content[..4].SequenceEqual("RIFF"u8)
            && content.Slice(8, 4).SequenceEqual("WEBP"u8);
    }
}
