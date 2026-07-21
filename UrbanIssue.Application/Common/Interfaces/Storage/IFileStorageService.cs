using UrbanIssue.Application.Common.Models;

namespace UrbanIssue.Application.Common.Interfaces.Storage;

public interface IFileStorageService
{
    Task<StoredFile> SaveAsync(
        string folder,
        UploadFile file,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken);
}
