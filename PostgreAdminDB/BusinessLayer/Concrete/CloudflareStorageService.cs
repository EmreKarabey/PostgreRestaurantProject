using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using BusinessLayer.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace BusinessLayer.Concrete
{
    public class CloudflareStorageService : ICloudflareStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _publicUrl; 

        public CloudflareStorageService(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _bucketName = configuration["CloudflareR2:BucketName"] ?? throw new Exception("CloudflareR2:BucketName bağlantısı bulunamadı");

            _publicUrl = configuration["CloudflareR2:PublicUrl"] ?? throw new Exception("CloudflareR2:PublicUrl bağlantısı bulunamadı");
        }

        public async Task<bool> DeleteImageAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            string objectKey = fileName.Trim();
            if (objectKey.StartsWith(_publicUrl))
            {
                objectKey = objectKey.Replace($"{_publicUrl}/", "");
            }
            else if (Uri.TryCreate(objectKey, UriKind.Absolute, out var uri))
            {
                objectKey = uri.AbsolutePath.TrimStart('/');
            }

            try
            {
                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = objectKey
                };

                var response = await _s3Client.DeleteObjectAsync(request);
                var deleted = response.HttpStatusCode == HttpStatusCode.NoContent || response.HttpStatusCode == HttpStatusCode.OK;
                if (deleted)
                {
                    var thumbnailExtension = Path.GetExtension(objectKey);
                    var thumbnailBaseKey = string.IsNullOrEmpty(thumbnailExtension)
                        ? objectKey
                        : objectKey[..^thumbnailExtension.Length];
                    await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = $"{thumbnailBaseKey}.thumb.webp"
                    });
                }

                return deleted;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Yüklenecek geçerli bir dosya bulunamadı.");

            string extension = Path.GetExtension(file.FileName);
            string uniqueFileName = $"images/{folder}/{Guid.NewGuid()}{extension}";

            using var stream = file.OpenReadStream();
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = uniqueFileName,
                InputStream = stream,
                ContentType = file.ContentType,
                DisablePayloadSigning = true
            };

            var response = await _s3Client.PutObjectAsync(request);

            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                await TryUploadThumbnailAsync(file, uniqueFileName);
                return $"{_publicUrl}/{uniqueFileName}";
            }

            throw new Exception("Dosya Cloudflare R2'ye yüklenirken bir sorun oluştu.");
        }

        private async Task TryUploadThumbnailAsync(IFormFile file, string originalKey)
        {
            try
            {
                await using var source = file.OpenReadStream();
                using var image = await Image.LoadAsync(source);
                image.Mutate(operation => operation.Resize(new ResizeOptions
                {
                    Size = new Size(480, 480),
                    Mode = ResizeMode.Crop,
                    Position = AnchorPositionMode.Center
                }));

                await using var thumbnail = new MemoryStream();
                await image.SaveAsync(thumbnail, new WebpEncoder { Quality = 72 });
                thumbnail.Position = 0;

                var thumbnailKey = GetThumbnailKey(originalKey);
                await _s3Client.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = thumbnailKey,
                    InputStream = thumbnail,
                    ContentType = "image/webp",
                    DisablePayloadSigning = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Görsel thumbnail'i oluşturulamadı: {ex.Message}");
            }
        }

        private static string GetThumbnailKey(string originalKey)
        {
            var extension = Path.GetExtension(originalKey);
            var baseKey = string.IsNullOrEmpty(extension)
                ? originalKey
                : originalKey[..^extension.Length];
            return $"{baseKey}.thumb.webp";
        }

        public async Task<(Stream? Stream, string? ContentType)> GetImageAsync(string fileUrlOrKey)
        {
            if (string.IsNullOrWhiteSpace(fileUrlOrKey))
                return (null, null);

            string objectKey = fileUrlOrKey.Trim();
            if (Uri.TryCreate(objectKey, UriKind.Absolute, out var uri))
            {
                objectKey = uri.AbsolutePath.TrimStart('/');
            }

            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = objectKey
                };

                var response = await _s3Client.GetObjectAsync(request);
                return (response.ResponseStream, response.Headers.ContentType);
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return (null, null);
            }
            catch
            {
                return (null, null);
            }
        }
    }
}
