using System;
using System.Threading.Tasks;
using CofeeAdmin.Services.Jwt;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CofeeAdmin.Controllers
{
    public class LoginController : Controller
    {
        private readonly IJwtService _jwtService;
        private readonly UserManager<AppUser> _userManager;

        public LoginController(IJwtService jwtService, UserManager<AppUser> userManager)
        {
            _jwtService = jwtService;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string username, string password, bool rememberMe = false)
        {
            var user = await _userManager.FindByNameAsync(username) ?? await _userManager.FindByEmailAsync(username) ;

            if (user == null)
            {
                TempData["ErrorMessage"] = "Giriş Yapılamadı kullanıcı adı/maili yada şifreniz yanlış! ";
                return View();
            }

            var passwordCheck = await _userManager.CheckPasswordAsync(user,password);

            if (!passwordCheck)
            {
                TempData["ErrorMessage"] = "Giriş Yapılamadı kullanıcı adı/maili yada şifreniz yanlış! ";
                return View();
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var token = _jwtService.GenerateToken(user.UserName, userRoles.FirstOrDefault());

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax
            };

            if (rememberMe)
            {
                cookieOptions.Expires = DateTimeOffset.UtcNow.AddDays(7);
            }

            Response.Cookies.Append("AccessToken", token, cookieOptions);


            if (!userRoles.Contains("Admin"))
            {
                return RedirectToAction("AccessDenied", "Error");
            }

            TempData["SuccessMessage"] = $"Hoş geldiniz, {user}!";
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AccessToken");

            TempData["SuccessMessage"] = "Oturum başarıyla sonlandırıldı.";

            return RedirectToAction("Index", "Login");
        }
    }
}
