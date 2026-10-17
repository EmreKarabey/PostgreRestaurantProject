using System.Threading.Tasks;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CofeeAdmin.ViewComponents
{
    public class _UserRolesPartialComponent : ViewComponent
    {
        private readonly UserManager<AppUser> _userManager;

        public _UserRolesPartialComponent(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                return View(new List<AppUser>());
            }

            var list = await _userManager.GetUsersInRoleAsync(roleName);
            return View(list?.ToList() ?? new List<AppUser>());
        }
    }
}
