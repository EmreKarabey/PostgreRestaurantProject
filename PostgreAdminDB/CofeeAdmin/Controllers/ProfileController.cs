using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BusinessLayer.Abstract;
using CofeeAdmin.Dto.AppUser;
using CofeeAdmin.Services.Jwt;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CofeeAdmin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ICloudflareStorageService _storageService;
        private readonly IJwtService _jwtService;

        public ProfileController(
            UserManager<AppUser> userManager,
            ICloudflareStorageService storageService,
            IJwtService jwtService)
        {
            _userManager = userManager;
            _storageService = storageService;
            _jwtService = jwtService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
            if (user == null) return RedirectToAction("Logout", "Login");

            return View(new MyProfileDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                ProfileImageURL = user.ProfileImageURL
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(MyProfileDto profile)
        {
            var user = await _userManager.FindByNameAsync(User.Identity?.Name ?? string.Empty);
            if (user == null) return RedirectToAction("Logout", "Login");

            profile.ProfileImageURL = user.ProfileImageURL;
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (profile.Image is { Length: > 0 })
            {
                var extension = Path.GetExtension(profile.Image.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension) || profile.Image.Length > 5 * 1024 * 1024)
                    ModelState.AddModelError(nameof(profile.Image), "JPG, PNG veya WEBP formatında, en fazla 5 MB boyutunda bir görsel seçin.");
            }

            if (!ModelState.IsValid) return View(profile);

            string? newImageUrl = null;
            if (profile.Image is { Length: > 0 })
            {
                try
                {
                    newImageUrl = await _storageService.UploadImageAsync(profile.Image, "user");
                }
                catch
                {
                    ModelState.AddModelError(nameof(profile.Image), "Profil fotoğrafı yüklenemedi. Lütfen tekrar deneyin.");
                    return View(profile);
                }
            }

            var oldImageUrl = user.ProfileImageURL;
            user.FirstName = profile.FirstName.Trim();
            user.LastName = profile.LastName.Trim();
            user.UserName = profile.UserName.Trim();
            user.Email = profile.Email.Trim();
            if (newImageUrl != null) user.ProfileImageURL = newImageUrl;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                if (newImageUrl != null) await _storageService.DeleteImageAsync(newImageUrl);
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                profile.ProfileImageURL = oldImageUrl;
                return View(profile);
            }

            if (newImageUrl != null && !string.IsNullOrWhiteSpace(oldImageUrl))
                await _storageService.DeleteImageAsync(oldImageUrl);

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtService.GenerateToken(user.UserName!, roles.Contains("Admin") ? "Admin" : roles.FirstOrDefault() ?? "Admin");
            Response.Cookies.Append("AccessToken", token, new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax
            });

            TempData["SuccessMessage"] = "Profil bilgileriniz güncellendi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
