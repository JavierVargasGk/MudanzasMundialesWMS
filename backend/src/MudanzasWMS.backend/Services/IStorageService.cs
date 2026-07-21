namespace MudanzasWMS.backend.Services;

public interface IStorageService
{
    Task<string> UploadAsync(string objectKey, Stream content, string contentType, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string objectKey, CancellationToken ct = default);
    Task<string> GetPresignedUrlAsync(string objectKey, TimeSpan expiry);
}
