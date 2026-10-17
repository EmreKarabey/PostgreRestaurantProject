using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.Abstract;
using CofeeAdmin.Dto.AppUser;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CofeeAdmin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagmentController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly ICloudflareStorageService _cloudflareStorageService;

        public UserManagmentController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IMapper mapper, ICloudflareStorageService cloudflareStorageService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _cloudflareStorageService = cloudflareStorageService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Roles = await _roleManager.Roles.OrderBy(x => x.Name).ToListAsync();
            var list = await _userManager.Users.ToListAsync();
            return View(list);
        }

        [HttpGet]
        public IActionResult AddUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(AddUserDto addUserDto)
        {
            if (!ModelState.IsValid) return View(addUserDto);

            if (addUserDto.Image == null || addUserDto.Image.Length == 0)
            {
                ModelState.AddModelError("Image", "Lütfen kullanıcı için bir görsel yükleyiniz.");
                return View(addUserDto);
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var fileExtension = Path.GetExtension(addUserDto.Image.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("Image", "Yalnızca JPG, JPEG, PNG veya WEBP formatında görsel yükleyebilirsiniz.");
                return View(addUserDto);
            }

            var appuser = _mapper.Map<AppUser>(addUserDto);

            try
            {
                var folder = "user";
                var imageURL = await _cloudflareStorageService.UploadImageAsync(addUserDto.Image, folder);
                appuser.ProfileImageURL = imageURL;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Image", $"Görsel Cloudflare R2'ye yüklenirken hata oluştu: {ex.Message}");
                return View(addUserDto);
            }

            var result = await _userManager.CreateAsync(appuser, addUserDto.Password);

            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    string turkishMessage = TranslateIdentityError(item.Code, item.Description);

                    if (item.Code.Contains("Password", StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError(nameof(addUserDto.Password), turkishMessage);
                    }
                    else if (item.Code.Contains("UserName", StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError(nameof(addUserDto.UserName), turkishMessage);
                    }
                    else if (item.Code.Contains("Email", StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError(nameof(addUserDto.Email), turkishMessage);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, turkishMessage);
                    }
                }
                return View(addUserDto);
            }

            TempData["SuccessMessage"] = $"\"{appuser.UserName}\" kullanıcısı başarıyla eklendi.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateUser(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Geçersiz kullanıcı kimliği.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                TempData["ErrorMessage"] = "Güncellenmek istenen kullanıcı bulunamadı.";
                return RedirectToAction("Index");
            }

            var updateUserDto = _mapper.Map<UpdateUserDto>(user);
            return View(updateUserDto);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser([FromForm] UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
            {
                return View(updateUserDto);
            }

            var user = await _userManager.FindByIdAsync(updateUserDto.Id.ToString());
            if (user == null)
            {
                TempData["ErrorMessage"] = "Güncellenmek istenen kullanıcı mevcut değil.";
                return RedirectToAction("Index");
            }

            if (updateUserDto.Image != null && updateUserDto.Image.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var fileExtension = Path.GetExtension(updateUserDto.Image.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Image", "Yalnızca JPG, JPEG, PNG veya WEBP formatında görsel yükleyebilirsiniz.");
                    return View(updateUserDto);
                }

                if (!string.IsNullOrWhiteSpace(user.ProfileImageURL))
                {
                    try
                    {
                        await _cloudflareStorageService.DeleteImageAsync(user.ProfileImageURL);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Eski profil görseli silinirken hata: {ex.Message}");
                    }
                }

                try
                {
                    var folder = "user";
                    var imageURL = await _cloudflareStorageService.UploadImageAsync(updateUserDto.Image, folder);
                    user.ProfileImageURL = imageURL;
                    updateUserDto.ProfileImageURL = imageURL;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("Image", $"Yeni görsel yüklenirken hata oluştu: {ex.Message}");
                    return View(updateUserDto);
                }
            }


            _mapper.Map(updateUserDto, user);

            if (!string.IsNullOrWhiteSpace(updateUserDto.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passResult = await _userManager.ResetPasswordAsync(user, token, updateUserDto.NewPassword);

                if (!passResult.Succeeded)
                {
                    foreach (var item in passResult.Errors)
                    {
                        string turkishMessage = TranslateIdentityError(item.Code, item.Description);
                        ModelState.AddModelError(nameof(updateUserDto.NewPassword), turkishMessage);
                    }
                    return View(updateUserDto);
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    string turkishMessage = TranslateIdentityError(item.Code, item.Description);
                    ModelState.AddModelError(string.Empty, turkishMessage);
                }
                return View(updateUserDto);
            }

            TempData["SuccessMessage"] = $"\"{user.UserName}\" kullanıcısı başarıyla güncellendi.";
            return RedirectToAction("Index");
        }

        [HttpPost, HttpGet]
        public async Task<IActionResult> DeleteUser(int id, string? returnURL = null)
        {
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (id <= 0)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Geçersiz kullanıcı ID'si." });

                TempData["ErrorMessage"] = "Geçersiz kullanıcı ID'si.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Silinmek istenen kullanıcı sistemde bulunamadı." });

                TempData["ErrorMessage"] = "Silinmek istenen kullanıcı sistemde bulunamadı.";
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrWhiteSpace(user.ProfileImageURL))
            {
                try
                {
                    await _cloudflareStorageService.DeleteImageAsync(user.ProfileImageURL);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Kullanıcı profil fotoğrafı Cloudflare üzerinden silinirken hata: {ex.Message}");
                }
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                var successMsg = $"\"{user.UserName}\" adlı kullanıcı başarıyla silindi.";
                TempData["SuccessMessage"] = successMsg;

                if (isAjax)
                    return Json(new { success = true, message = successMsg });
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => TranslateIdentityError(e.Code, e.Description)));
                if (isAjax)
                    return Json(new { success = false, message = $"Kullanıcı silinemedi: {errors}" });

                TempData["ErrorMessage"] = $"Kullanıcı silinemedi: {errors}";
            }

            if (!string.IsNullOrEmpty(returnURL) && Url.IsLocalUrl(returnURL))
            {
                return Redirect(returnURL);
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> AssigneRole(string role, int userId)
        {
            if (string.IsNullOrEmpty(role))
            {
                return RedirectToAction("Index");
            }

            if (userId < 0)
            {
                TempData["ErrorMessage"] = $"Kullanıcı id geçersiz değer.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                TempData["ErrorMessage"] = $"Kullanıcı bulunamadı.";
                return RedirectToAction("Index");
            }

            var result = await _userManager.AddToRoleAsync(user, role);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = $"Kullanıcıya rol atanmılamadı.";
                return RedirectToAction("Index");
            }
            var successMsg = $"\"{user.UserName}\" adlı kullanıcıya başarıyla {role} atandı.";
            TempData["SuccessMessage"] = successMsg;
            return RedirectToAction("Index");
        }

        private static string TranslateIdentityError(string code, string defaultDesc)
        {
            return code switch
            {
                "DuplicateUserName" => "Bu kullanıcı adı sistemde zaten kayıtlı.",
                "DuplicateEmail" => "Bu e-posta adresi sistemde zaten kayıtlı.",
                "PasswordTooShort" => "Şifre en az 6 karakter uzunluğunda olmalıdır.",
                "PasswordRequiresNonAlphanumeric" => "Şifreniz en az bir özel karakter (@, #, $, !, . vb.) içermelidir.",
                "PasswordRequiresDigit" => "Şifreniz en az bir rakam ('0'-'9') içermelidir.",
                "PasswordRequiresLower" => "Şifreniz en az bir küçük harf ('a'-'z') içermelidir.",
                "PasswordRequiresUpper" => "Şifreniz en az bir büyük harf ('A'-'Z') içermelidir.",
                "InvalidUserName" => "Kullanıcı adı yalnızca harf, rakam ve izin verilen karakterleri içerebilir.",
                "InvalidEmail" => "Lütfen geçerli bir e-posta formatı giriniz.",
                _ => defaultDesc
            };
        }
    }
}