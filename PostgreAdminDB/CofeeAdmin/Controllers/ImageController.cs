using BusinessLayer.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CofeeAdmin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ImageController : Controller
    {
        private readonly ICloudflareStorageService _cloudflareStorageService;

        public ImageController(ICloudflareStorageService cloudflareStorageService)
        {
            _cloudflareStorageService = cloudflareStorageService;
        }

        [HttpGet]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> Get([FromQuery] string url, [FromQuery] bool thumbnail = false)
        {
            if (string.IsNullOrWhiteSpace(url))
                return NotFound();

            var imageKey = thumbnail ? GetThumbnailKey(url) : url;
            var (stream, contentType) = await _cloudflareStorageService.GetImageAsync(imageKey);
            if (stream == null && thumbnail)
            {
                // Thumbnail'i önceden oluşturulmamış eski dosyalarda orijinali göster.
                (stream, contentType) = await _cloudflareStorageService.GetImageAsync(url);
            }

            if (stream != null)
            {
                return File(stream, contentType ?? (thumbnail ? "image/webp" : "image/jpeg"));
            }

            if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && !url.Contains("r2.dev"))
            {
                return Redirect(url);
            }

            return NotFound();
        }

        private static string GetThumbnailKey(string fileUrlOrKey)
        {
            var key = Uri.TryCreate(fileUrlOrKey, UriKind.Absolute, out var uri)
                ? uri.AbsolutePath.TrimStart('/')
                : fileUrlOrKey.Trim().TrimStart('/');
            var extension = Path.GetExtension(key);
            var baseKey = string.IsNullOrEmpty(extension) ? key : key[..^extension.Length];
            return $"{baseKey}.thumb.webp";
        }
    }
}
