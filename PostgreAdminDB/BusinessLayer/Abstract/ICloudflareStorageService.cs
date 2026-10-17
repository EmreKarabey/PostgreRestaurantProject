using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BusinessLayer.Abstract
{
    public interface ICloudflareStorageService
    {
        Task<string> UploadImageAsync(IFormFile file, string folder);
        Task<bool> DeleteImageAsync(string fileName);
        Task<(Stream? Stream, string? ContentType)> GetImageAsync(string fileUrlOrKey);
    }
}
