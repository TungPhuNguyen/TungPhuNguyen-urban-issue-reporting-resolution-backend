namespace UrbanIssue.Application.Common.Models;

public sealed record UploadFile(
    string FileName,
    string ContentType,
    long Length,
    Stream Content);
