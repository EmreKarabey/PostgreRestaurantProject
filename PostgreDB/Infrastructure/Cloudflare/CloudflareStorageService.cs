using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Application.Services.Cloudflare;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.CloudflareService
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
                return response.HttpStatusCode == HttpStatusCode.NoContent || response.HttpStatusCode == HttpStatusCode.OK;
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
                return $"{_publicUrl}/{uniqueFileName}";
            }

            throw new Exception("Dosya Cloudflare R2'ye yüklenirken bir sorun oluştu.");
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
