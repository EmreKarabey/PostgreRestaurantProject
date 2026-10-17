using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CofeeAdmin.Dto.AppRole;
using CofeeAdmin.Dto.AppUser;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CofeeAdmin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IMapper _mapper;
        public RoleController(RoleManager<AppRole> roleManager, IMapper mapper)
        {
            _roleManager = roleManager;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var list = await _roleManager.Roles.ToListAsync();
            return View(list);
        }


        [HttpGet]
        public IActionResult AddRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddRole(AddRoleDto addRoleDto)
        {
            if (!ModelState.IsValid) return View(addRoleDto);

            var approle = _mapper.Map<AppRole>(addRoleDto);

            var result = await _roleManager.CreateAsync(approle);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "Eklemek istediğiniz rol eklenilemedi.";
                return View(addRoleDto);
            }

            TempData["SuccessMessage"] = $"\"{approle.Name}\" rolü başarıyla eklendi.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateRole(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Geçersiz rol ID'si.";
                return RedirectToAction("Index");
            }

            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                TempData["ErrorMessage"] = "Düzenlenmek istenen rol bulunamadı.";
                return RedirectToAction("Index");
            }

            var dto = _mapper.Map<UpdateRoleDto>(role);
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(UpdateRoleDto updateRoleDto)
        {
            if (!ModelState.IsValid)
            {
                return View(updateRoleDto);
            }

            var role = await _roleManager.FindByIdAsync(updateRoleDto.Id.ToString());
            if (role == null)
            {
                TempData["ErrorMessage"] = "Güncellenmek istenen rol sistemde bulunamadı.";
                return RedirectToAction("Index");
            }

            _mapper.Map(updateRoleDto, role);

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "Rol güncellenirken hata oluştu.";
                return View(updateRoleDto);
            }

            TempData["SuccessMessage"] = $"\"{role.Name}\" rolü başarıyla güncellendi.";
            return RedirectToAction("Index");
        }

        [HttpPost, HttpGet]
        public async Task<IActionResult> DeleteRole(int id, string? returnURL = null)
        {
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (id <= 0)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Geçersiz rol ID'si." });

                TempData["ErrorMessage"] = "Geçersiz rol ID'si.";
                return RedirectToAction("Index");
            }

            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Silinmek istenen rol sistemde bulunamadı." });

                TempData["ErrorMessage"] = "Silinmek istenen rol sistemde bulunamadı.";
                return RedirectToAction("Index");
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                var successMsg = $"\"{role.Name}\" rolü başarıyla silindi.";
                TempData["SuccessMessage"] = successMsg;

                if (isAjax)
                    return Json(new { success = true, message = successMsg });
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                if (isAjax)
                    return Json(new { success = false, message = $"Rol silinemedi: {errors}" });

                TempData["ErrorMessage"] = $"Rol silinemedi: {errors}";
            }

            if (!string.IsNullOrEmpty(returnURL) && Url.IsLocalUrl(returnURL))
            {
                return Redirect(returnURL);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult GetRoleUsersComponent(string roleName)
        {
            return ViewComponent("_UserRolesPartialComponent", new { roleName = roleName });
        }

   
    }
}
