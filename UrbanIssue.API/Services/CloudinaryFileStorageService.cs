using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using System.Security.AccessControl;
using UrbanIssue.API.Settings;
using UrbanIssue.Application.Common.Interfaces.Storage;
using UrbanIssue.Application.Common.Models;
using ResourceType = System.Security.AccessControl.ResourceType;

namespace UrbanIssue.API.Services;

public sealed class CloudinaryFileStorageService
    : IFileStorageService
{
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

    private readonly Cloudinary _cloudinary;

    public CloudinaryFileStorageService(
        IOptions<CloudinarySettings> options)
    {
        var settings = options.Value;

        if (string.IsNullOrWhiteSpace(settings.CloudName)
            || string.IsNullOrWhiteSpace(settings.ApiKey)
            || string.IsNullOrWhiteSpace(settings.ApiSecret))
        {
            throw new InvalidOperationException(
                "Cloudinary chưa được cấu hình đầy đủ "
                + "CloudName, ApiKey và ApiSecret.");
        }

        _cloudinary = new Cloudinary(
            new Account(
                settings.CloudName.Trim(),
                settings.ApiKey.Trim(),
                settings.ApiSecret.Trim()));

        _cloudinary.Api.Secure = true;
    }

    public async Task<StoredFile> SaveAsync(
        string folder,
        UploadFile file,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (file.Length <= 0)
        {
            throw new InvalidOperationException(
                "File ảnh không có dữ liệu.");
        }

        var extension = Path.GetExtension(file.FileName);

        if (string.IsNullOrWhiteSpace(extension)
            || !AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                "Chỉ hỗ trợ ảnh JPG, JPEG, PNG hoặc WEBP.");
        }

        await using var buffer = new MemoryStream();

        if (file.Content.CanSeek)
        {
            file.Content.Position = 0;
        }

        await file.Content.CopyToAsync(
            buffer,
            cancellationToken);

        var bytes = buffer.ToArray();

        if (!HasExpectedImageSignature(bytes, extension))
        {
            throw new InvalidOperationException(
                "Nội dung file không khớp với định dạng "
                + "ảnh JPG, PNG hoặc WEBP.");
        }

        buffer.Position = 0;

        var normalizedFolder = NormalizeFolder(folder);

        var uploadParameters = new ImageUploadParams
        {
            File = new FileDescription(
                file.FileName,
                buffer),

            Folder = normalizedFolder,
            PublicId = Guid.NewGuid().ToString("N"),
            Overwrite = false
        };

        ImageUploadResult uploadResult;

        try
        {
            uploadResult = await _cloudinary.UploadAsync(
                uploadParameters,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                "Không thể kết nối tới Cloudinary để tải ảnh.",
                exception);
        }

        if (uploadResult.Error is not null)
        {
            throw new InvalidOperationException(
                "Không thể tải ảnh lên Cloudinary: "
                + uploadResult.Error.Message);
        }

        var publicId = uploadResult.PublicId;
        var secureUrl = uploadResult.SecureUrl?.AbsoluteUri;

        if (string.IsNullOrWhiteSpace(publicId)
            || string.IsNullOrWhiteSpace(secureUrl))
        {
            throw new InvalidOperationException(
                "Cloudinary không trả về public_id "
                + "hoặc secure_url hợp lệ.");
        }

        return new StoredFile(
            StorageKey: publicId,
            PublicUrl: secureUrl);
    }

    public async Task DeleteAsync(
    string storageKey,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        DeletionResult deletionResult;

        try
        {
            deletionResult = await _cloudinary.DestroyAsync(
                new DeletionParams(storageKey)
                {
                    Invalidate = true
                });
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                "Không thể kết nối tới Cloudinary để xóa ảnh.",
                exception);
        }

        if (deletionResult.Error is not null)
        {
            throw new InvalidOperationException(
                "Không thể xóa ảnh khỏi Cloudinary: "
                + deletionResult.Error.Message);
        }
    }

    private static string NormalizeFolder(string folder)
    {
        var normalized = string.IsNullOrWhiteSpace(folder)
            ? "urban-issue"
            : folder
                .Trim()
                .Replace('\\', '/')
                .Trim('/');

        while (normalized.Contains("//", StringComparison.Ordinal))
        {
            normalized = normalized.Replace(
                "//",
                "/",
                StringComparison.Ordinal);
        }

        return string.IsNullOrWhiteSpace(normalized)
            ? "urban-issue"
            : normalized;
    }

    private static bool HasExpectedImageSignature(
        ReadOnlySpan<byte> content,
        string extension)
    {
        if (extension.Equals(
                ".jpg",
                StringComparison.OrdinalIgnoreCase)
            || extension.Equals(
                ".jpeg",
                StringComparison.OrdinalIgnoreCase))
        {
            return content.Length >= 3
                && content[0] == 0xFF
                && content[1] == 0xD8
                && content[2] == 0xFF;
        }

        if (extension.Equals(
                ".png",
                StringComparison.OrdinalIgnoreCase))
        {
            ReadOnlySpan<byte> pngSignature =
                [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

            return content.StartsWith(pngSignature);
        }

        return content.Length >= 12
            && content[..4].SequenceEqual("RIFF"u8)
            && content.Slice(8, 4).SequenceEqual("WEBP"u8);
    }
}
