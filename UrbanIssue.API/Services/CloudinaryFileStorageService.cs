using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using UrbanIssue.API.Settings;
using UrbanIssue.Application.Common.Interfaces.Storage;
using UrbanIssue.Application.Common.Models;

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

    private readonly HttpClient _httpClient;
    private readonly CloudinarySettings _settings;

    public CloudinaryFileStorageService(
        HttpClient httpClient,
        IOptions<CloudinarySettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;

        if (string.IsNullOrWhiteSpace(_settings.CloudName)
            || string.IsNullOrWhiteSpace(_settings.ApiKey)
            || string.IsNullOrWhiteSpace(_settings.ApiSecret))
        {
            throw new InvalidOperationException(
                "Cloudinary chưa được cấu hình đầy đủ CloudName, ApiKey và ApiSecret.");
        }
    }

    public async Task<StoredFile> SaveAsync(
        string folder,
        UploadFile file,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(file.FileName);

        if (string.IsNullOrWhiteSpace(extension)
            || !AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                "Định dạng ảnh không được hỗ trợ.");
        }

        await using var contentStream = new MemoryStream();
        await file.Content.CopyToAsync(contentStream, cancellationToken);
        var bytes = contentStream.ToArray();

        if (!HasExpectedImageSignature(bytes, extension))
        {
            throw new InvalidOperationException(
                "Nội dung file không khớp với định dạng ảnh JPG, PNG hoặc WEBP.");
        }

        var timestamp = DateTimeOffset.UtcNow
            .ToUnixTimeSeconds()
            .ToString(CultureInfo.InvariantCulture);
        var normalizedFolder = folder.Trim('/');
        var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["folder"] = normalizedFolder,
            ["timestamp"] = timestamp
        };

        using var form = new MultipartFormDataContent();
        using var imageContent = new ByteArrayContent(bytes);
        imageContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

        form.Add(imageContent, "file", file.FileName);
        form.Add(new StringContent(_settings.ApiKey), "api_key");
        form.Add(new StringContent(timestamp), "timestamp");
        form.Add(new StringContent(normalizedFolder), "folder");
        form.Add(new StringContent(CreateSignature(parameters)), "signature");

        var endpoint =
            $"https://api.cloudinary.com/v1_1/{Uri.EscapeDataString(_settings.CloudName)}/image/upload";
        using var response = await _httpClient.PostAsync(
            endpoint,
            form,
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Không thể tải ảnh lên Cloudinary ({(int)response.StatusCode}): {ReadCloudinaryError(responseBody)}");
        }

        using var document = JsonDocument.Parse(responseBody);
        var root = document.RootElement;
        var publicId = root.GetProperty("public_id").GetString();
        var secureUrl = root.GetProperty("secure_url").GetString();

        if (string.IsNullOrWhiteSpace(publicId)
            || string.IsNullOrWhiteSpace(secureUrl))
        {
            throw new InvalidOperationException(
                "Cloudinary không trả về public_id hoặc secure_url hợp lệ.");
        }

        return new StoredFile(publicId, secureUrl);
    }

    public async Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            return;
        }

        var timestamp = DateTimeOffset.UtcNow
            .ToUnixTimeSeconds()
            .ToString(CultureInfo.InvariantCulture);
        var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["invalidate"] = "true",
            ["public_id"] = storageKey,
            ["timestamp"] = timestamp
        };

        using var form = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["api_key"] = _settings.ApiKey,
                ["invalidate"] = "true",
                ["public_id"] = storageKey,
                ["timestamp"] = timestamp,
                ["signature"] = CreateSignature(parameters)
            });

        var endpoint =
            $"https://api.cloudinary.com/v1_1/{Uri.EscapeDataString(_settings.CloudName)}/image/destroy";
        using var response = await _httpClient.PostAsync(
            endpoint,
            form,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Không thể xóa ảnh khỏi Cloudinary ({(int)response.StatusCode}): {ReadCloudinaryError(responseBody)}");
        }
    }

    private string CreateSignature(
        IReadOnlyDictionary<string, string> parameters)
    {
        var value = string.Join(
            "&",
            parameters.Select(parameter => $"{parameter.Key}={parameter.Value}"));
        var bytes = SHA1.HashData(
            Encoding.UTF8.GetBytes(value + _settings.ApiSecret));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string ReadCloudinaryError(string responseBody)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBody);
            if (document.RootElement.TryGetProperty("error", out var error)
                && error.TryGetProperty("message", out var message))
            {
                return message.GetString() ?? "Lỗi không xác định.";
            }

            return "Lỗi không xác định.";
        }
        catch (JsonException)
        {
            return "Lỗi không xác định.";
        }
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
