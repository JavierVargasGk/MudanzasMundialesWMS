using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Options;

namespace MudanzasWMS.backend.Services;


public class MinioStorageService : IStorageService
{
    private readonly IAmazonS3 _client;
    private readonly string _bucket;

    public MinioStorageService(IOptions<MinioOptions> options)
    {
        var cfg = options.Value;
        _bucket = cfg.Bucket;

        var scheme = cfg.UseSsl ? "https://" : "http://";
        var s3Config = new AmazonS3Config
        {
            ServiceURL = scheme + cfg.Endpoint,
            ForcePathStyle = true
        };
        _client = new AmazonS3Client(cfg.AccessKey, cfg.SecretKey, s3Config);
    }

    public async Task<string> UploadAsync(string objectKey, Stream content, string contentType, CancellationToken ct = default)
    {
        await EnsureBucketAsync(ct);
        var request = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = objectKey,
            InputStream = content,
            ContentType = contentType
        };
        await _client.PutObjectAsync(request, ct);
        return objectKey;
    }

    public async Task<Stream> DownloadAsync(string objectKey, CancellationToken ct = default)
    {
        var response = await _client.GetObjectAsync(_bucket, objectKey, ct);
        return response.ResponseStream;
    }

    public Task<string> GetPresignedUrlAsync(string objectKey, TimeSpan expiry)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucket,
            Key = objectKey,
            Expires = DateTime.UtcNow.Add(expiry)
        };
        return Task.FromResult(_client.GetPreSignedURL(request));
    }

    private async Task EnsureBucketAsync(CancellationToken ct)
    {
        var exists = await AmazonS3Util.DoesS3BucketExistV2Async(_client, _bucket);
        if (!exists)
        {
            await _client.PutBucketAsync(new PutBucketRequest { BucketName = _bucket }, ct);
        }
    }
}
