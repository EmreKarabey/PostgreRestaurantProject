using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Identity;
using EntityLayer.Entities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CofeeAdmin.ViewComponents
{
    public class _NavbarPartialComponent:ViewComponent
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public _NavbarPartialComponent(
            UserManager<AppUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var user = string.IsNullOrWhiteSpace(username)
                ? null
                : await _userManager.FindByNameAsync(username);

            return View(user);
        }
    }
}
