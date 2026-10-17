using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.Abstract;
using CofeeAdmin.Dto.Category;
using CofeeAdmin.SignalR;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CofeeAdmin.Controllers
{
    [Authorize(Roles ="Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly ICloudflareStorageService _cloudflareStorageService;
        private readonly IMapper _mapper;
        private readonly IHubContext<SignalRHub> _hubContext;
        private readonly IProductService _productService;
        public CategoryController(ICategoryService categoryService, ICloudflareStorageService cloudflareStorageService, IMapper mapper, IHubContext<SignalRHub> hubContext, IProductService productService)
        {
            _categoryService = categoryService;
            _cloudflareStorageService = cloudflareStorageService;
            _mapper = mapper;
            _hubContext = hubContext;
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int size = 15, int page = 1)
        {
            var list = await _categoryService.GetListAsync(page: page, pageSize: size);
            return View(list);
        }

        [HttpGet]
        public IActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory([FromForm] AddCategoryDto addCategoryDto)
        {
            if (!ModelState.IsValid)
            {
                return View(addCategoryDto);
            }

            if (addCategoryDto.Image == null || addCategoryDto.Image.Length == 0)
            {
                ModelState.AddModelError("Image", "Lütfen kategori için bir görsel yükleyiniz.");
                return View(addCategoryDto);
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var fileExtension = Path.GetExtension(addCategoryDto.Image.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("Image", "Yalnızca JPG, JPEG, PNG veya WEBP formatında görsel yükleyebilirsiniz.");
                return View(addCategoryDto);
            }


            var entity = _mapper.Map<Category>(addCategoryDto);

            entity.CreatedAt = DateTime.UtcNow;

            try
            {
                var folder = "category";
                var imageURL = await _cloudflareStorageService.UploadImageAsync(addCategoryDto.Image, folder);
                entity.ImageURL = imageURL;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Image", $"Görsel Cloudflare R2'ye yüklenirken hata oluştu: {ex.Message}");
                return View(addCategoryDto);
            }


            try
            {
                await _categoryService.AddAsync(entity);
                TempData["SuccessMessage"] = $"\"{entity.CategoryName}\" kategorisi başarıyla eklendi.";

                if (entity.DeletedAt == null && entity.Status)
                {
                    var totalCategories = await _categoryService.CountAsync(predicate: n => n.DeletedAt == null && n.Status == true);
                    await _hubContext.Clients.All.SendAsync("TotalCategories", totalCategories);
                }
                await _hubContext.Clients.All.SendAsync("CategoryStatisticsChanged");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Kategori kaydedilirken veritabanı hatası oluştu: {ex.Message}");
                return View(addCategoryDto);
            }
        }



        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id, string? returnURL = null)
        {
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (id <= 0)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Geçersiz kategori ID'si." });

                ModelState.AddModelError("EntityID", "Id alanı boş.");
                return RedirectToAction("Index");
            }

            var entity = await _categoryService.GetByIdAsync(id);

            if (entity == null)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Silmek istediğiniz veri mevcut değil." });

                TempData["ErrorMessage"] = "Silmek istediğiniz veri mevcut değil.";
                return RedirectToAction("Index");
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(entity.ImageURL))
                {
                    await _cloudflareStorageService.DeleteImageAsync(entity.ImageURL);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Görsel silinirken hata: {ex.Message}");
            }

            try
            {
                await _categoryService.DeleteAsync(entity);
                var successMsg = $"\"{entity.CategoryName}\" kategorisi başarıyla silindi.";

                if (entity.DeletedAt == null && entity.Status)
                {
                    var totalCategories = await _categoryService.CountAsync(predicate: n => n.DeletedAt == null && n.Status == true);
                    await _hubContext.Clients.All.SendAsync("TotalCategories", totalCategories);
                }

                var categoriesList = await _productService.CategoryWithProducts();
                await _hubContext.Clients.All.SendAsync("CategoriesList", categoriesList);
                await _hubContext.Clients.All.SendAsync("CategoryStatisticsChanged");

                TempData["SuccessMessage"] = successMsg;

                if (isAjax)
                    return Json(new { success = true, message = successMsg });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kategori silinirken hata: {ex.Message}");
                if (isAjax)
                    return Json(new { success = false, message = $"Kategori silinirken hata oluştu: {ex.Message}" });
            }

            if (!string.IsNullOrEmpty(returnURL) && Url.IsLocalUrl(returnURL))
            {
                return Redirect(returnURL);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateCategory(int id)
        {
            if (id <= 0)
            {
                ModelState.AddModelError("EntityID", "Id alanı boş.");
                return RedirectToAction("Index");
            }

            var entity = await _categoryService.GetByIdAsync(id);

            if (entity == null)
            {
                TempData["ErrorMessage"] = "Güncellemek istediğiniz veri mevcut değil.";
                return RedirectToAction("Index");
            }

            var updateCategory = _mapper.Map<UpdateCategoryDto>(entity);

            return View(updateCategory);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory([FromForm] UpdateCategoryDto updateCategoryDto)
        {
            if (!ModelState.IsValid)
            {
                return View(updateCategoryDto);
            }

            var entity = await _categoryService.GetByIdAsync(updateCategoryDto.EntityID);


            if (entity == null)
            {
                TempData["ErrorMessage"] = "Güncellemek istediğiniz veri mevcut değil.";
                return RedirectToAction("Index");
            }


            if (updateCategoryDto.Image != null && updateCategoryDto.Image.Length > 0)
            {
                if (!string.IsNullOrWhiteSpace(entity.ImageURL))
                {
                    try
                    {
                        await _cloudflareStorageService.DeleteImageAsync(entity.ImageURL);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Eski görsel silinirken hata: {ex.Message}");
                    }
                }

                var folder = "category";
                var imageURL = await _cloudflareStorageService.UploadImageAsync(updateCategoryDto.Image, folder);
                updateCategoryDto.ImageURL = imageURL;
            }

            var statusChanged = entity.Status != updateCategoryDto.Status;
            var nameChanged = entity.CategoryName != updateCategoryDto.CategoryName;
            var update = _mapper.Map(updateCategoryDto, entity);

            update.UpdateAt = DateTime.UtcNow;

            await _categoryService.UpdateAsync(update);

            TempData["SuccessMessage"] = $"\"{entity.CategoryName}\" kategorisi başarıyla güncellendi.";

            if (statusChanged)
            {
                if (entity.DeletedAt == null)
                {
                    var totalCategories = await _categoryService.CountAsync(predicate: n => n.DeletedAt == null && n.Status == true);
                    await _hubContext.Clients.All.SendAsync("TotalCategories", totalCategories);
                }
                await _hubContext.Clients.All.SendAsync("CategoryStatisticsChanged");
            }

            if (nameChanged)
            {
                var categoriesList = await _productService.CategoryWithProducts();
                await _hubContext.Clients.All.SendAsync("CategoriesList", categoriesList);
            }

            return RedirectToAction("Index");

        }


        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, string? returnURL = null)
        {
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (id <= 0)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Geçersiz kategori ID'si." });

                ModelState.AddModelError("EntityID", "Id alanı boş.");
                return RedirectToAction("Index");
            }

            var entity = await _categoryService.GetByIdAsync(id);

            if (entity == null)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Silmek istediğiniz veri mevcut değil." });

                TempData["ErrorMessage"] = "Güncellemek istediğiniz veri mevcut değil.";
                return RedirectToAction("Index");
            }

            entity.Status = !entity.Status;
            entity.UpdateAt = DateTime.UtcNow;

            try
            {
                await _categoryService.UpdateAsync(entity);
                var successMsg = $"\"{entity.CategoryName}\" kategorisinin durumu başarılı bir şekilde değiştirildi.";

                if (entity.DeletedAt == null)
                {
                    var totalCategories = await _categoryService.CountAsync(predicate: n => n.DeletedAt == null && n.Status == true);
                    await _hubContext.Clients.All.SendAsync("TotalCategories", totalCategories);
                }

                await _hubContext.Clients.All.SendAsync("CategoryStatisticsChanged");

                TempData["SuccessMessage"] = successMsg;

                if (isAjax)
                    return Json(new { success = true, message = successMsg, newStatus = entity.Status });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kategori durumu güncellenirken hata: {ex.Message}");
                if (isAjax)
                    return Json(new { success = false, message = $"Kategori güncellenirken hata oluştu: {ex.Message}" });
            }

            if (!string.IsNullOrEmpty(returnURL) && Url.IsLocalUrl(returnURL))
            {
                return Redirect(returnURL);
            }

            return RedirectToAction("Index");


        }

    }
}
