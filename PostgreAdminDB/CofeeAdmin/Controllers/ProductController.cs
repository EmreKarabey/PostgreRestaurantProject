using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.Abstract;
using CofeeAdmin.Dto.Category;
using CofeeAdmin.Dto.Product;
using CofeeAdmin.SignalR;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CofeeAdmin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ICloudflareStorageService _cloudflareStorageService;
        private readonly IMapper _mapper;
        private readonly IHubContext<SignalRHub> _hubContext;

        public ProductController(IProductService productService, ICategoryService categoryService, ICloudflareStorageService cloudflareStorageService, IMapper mapper, IHubContext<SignalRHub> hubContext)
        {
            _productService = productService;
            _categoryService = categoryService;
            _cloudflareStorageService = cloudflareStorageService;
            _mapper = mapper;
            _hubContext = hubContext;
        }


        [HttpGet]
        public async Task<IActionResult> Index(int size = 15, int page = 1)
        {
            var list = await _productService.GetListAsync(
                include: query => query.Include(x => x.Category),
                orderBy: q => q.OrderByDescending(x => x.EntityID),
                page: page,
                pageSize: size
            );



            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, string? returnURL = null)
        {
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (id <= 0)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Geçersiz ürün ID'si." });

                ModelState.AddModelError("EntityID", "Id alanı boş.");
                return RedirectToAction("Index");
            }

            var entity = await _productService.GetByIdAsync(id);

            if (entity == null)
            {
                if (isAjax)
                    return Json(new { success = false, message = "İşlem yapmak istediğiniz ürün bulunamadı." });

                TempData["ErrorMessage"] = "Güncellemek istediğiniz ürün mevcut değil.";
                return RedirectToAction("Index");
            }

            entity.Status = !entity.Status;
            entity.UpdateAt = DateTime.UtcNow;

            try
            {
                await _productService.UpdateAsync(entity);
                var successMsg = $"\"{entity.ProductName}\" ürününün durumu başarılı bir şekilde değiştirildi.";
                TempData["SuccessMessage"] = successMsg;

                if (entity.DeletedAt == null)
                {
                    var totalProductsCount = await _productService.CountAsync(predicate: n => n.DeletedAt == null && n.Status == true);
                    await _hubContext.Clients.All.SendAsync("TotalProducts", totalProductsCount);
                }
                await _hubContext.Clients.All.SendAsync("ProductStatisticsChanged");

                if (isAjax)
                    return Json(new { success = true, message = successMsg, newStatus = entity.Status });


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürün durumu güncellenirken hata: {ex.Message}");
                if (isAjax)
                    return Json(new { success = false, message = $"Ürün güncellenirken hata oluştu: {ex.Message}" });
            }

            if (!string.IsNullOrEmpty(returnURL) && Url.IsLocalUrl(returnURL))
            {
                return Redirect(returnURL);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id, string? returnURL = null)
        {
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (id <= 0)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Geçersiz ürün ID'si." });

                ModelState.AddModelError("EntityID", "Id alanı boş.");
                return RedirectToAction("Index");
            }

            var entity = await _productService.GetByIdAsync(id);

            if (entity == null)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Silmek istediğiniz ürün bulunamadı." });

                TempData["ErrorMessage"] = "Silmek istediğiniz ürün mevcut değil.";
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
                Console.WriteLine($"Ürün görseli silinirken hata: {ex.Message}");
            }

            try
            {
                await _productService.DeleteAsync(entity);
                var successMsg = $"\"{entity.ProductName}\" ürünü başarıyla silindi.";
                TempData["SuccessMessage"] = successMsg;

                if (entity.DeletedAt == null && entity.Status)
                {
                    var totalProductsCount = await _productService.CountAsync(predicate: n => n.DeletedAt == null && n.Status == true);
                    await _hubContext.Clients.All.SendAsync("TotalProducts", totalProductsCount);
                }

                var categoriesList = await _productService.CategoryWithProducts();
                await _hubContext.Clients.All.SendAsync("CategoriesList", categoriesList);
                await _hubContext.Clients.All.SendAsync("ProductStatisticsChanged");
                await _hubContext.Clients.All.SendAsync("CategoryStatisticsChanged");

                if (isAjax)
                    return Json(new { success = true, message = successMsg });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürün silinirken hata: {ex.Message}");
                if (isAjax)
                    return Json(new { success = false, message = $"Ürün silinirken hata oluştu: {ex.Message}" });
            }

            if (!string.IsNullOrEmpty(returnURL) && Url.IsLocalUrl(returnURL))
            {
                return Redirect(returnURL);
            }

            return RedirectToAction("Index");
        }


        [HttpGet]
        public IActionResult AddProduct()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromForm] AddProductDto addProductDto)
        {
            if (!ModelState.IsValid)
            {
                return View(addProductDto);
            }

            if (addProductDto.Image == null || addProductDto.Image.Length == 0)
            {
                ModelState.AddModelError("Image", "Lütfen ürün için bir görsel yükleyiniz.");
                return View(addProductDto);
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var fileExtension = Path.GetExtension(addProductDto.Image.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("Image", "Yalnızca JPG, JPEG, PNG veya WEBP formatında görsel yükleyebilirsiniz.");
                return View(addProductDto);
            }


            var entity = _mapper.Map<Product>(addProductDto);

            entity.CreatedAt = DateTime.UtcNow;

            try
            {
                var folder = "product";
                var imageURL = await _cloudflareStorageService.UploadImageAsync(addProductDto.Image, folder);
                entity.ImageURL = imageURL;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Image", $"Görsel Cloudflare R2'ye yüklenirken hata oluştu: {ex.Message}");
                return View(addProductDto);
            }


            try
            {
                await _productService.AddAsync(entity);
                TempData["SuccessMessage"] = $"\"{entity.ProductName}\" ürünü başarıyla eklendi.";

                if (entity.Status)
                {
                    var totalProductsCount = await _productService.CountAsync(predicate: n => n.DeletedAt == null && n.Status == true);
                    await _hubContext.Clients.All.SendAsync("TotalProducts", totalProductsCount);
                }

                var categoriesList = await _productService.CategoryWithProducts();
                await _hubContext.Clients.All.SendAsync("CategoriesList", categoriesList);
                await _hubContext.Clients.All.SendAsync("ProductStatisticsChanged");
                await _hubContext.Clients.All.SendAsync("CategoryStatisticsChanged");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ürün kaydedilirken veritabanı hatası oluştu: {ex.Message}");
                return View(addProductDto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> UpdateProduct(int id)
        {
            if (id <= 0)
            {
                ModelState.AddModelError("EntityID", "Id alanı boş.");
                return RedirectToAction("Index");
            }

            var entity = await _productService.GetByIdAsync(id);

            if (entity == null)
            {
                TempData["ErrorMessage"] = "Güncellemek istediğiniz veri mevcut değil.";
                return RedirectToAction("Index");
            }

            var updateProduct = _mapper.Map<UpdateProductDto>(entity);

            return View(updateProduct);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProduct([FromForm] UpdateProductDto updateProductDto)
        {
            if (!ModelState.IsValid)
            {
                return View(updateProductDto);
            }

            var entity = await _productService.GetByIdAsync(updateProductDto.EntityID);

            if (entity == null)
            {
                TempData["ErrorMessage"] = "Güncellemek istediğiniz ürün mevcut değil.";
                return RedirectToAction("Index");
            }

            if (updateProductDto.Image != null && updateProductDto.Image.Length > 0)
            {
                if (!string.IsNullOrWhiteSpace(entity.ImageURL))
                {
                    try
                    {
                        await _cloudflareStorageService.DeleteImageAsync(entity.ImageURL);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Eski ürün görseli silinirken hata: {ex.Message}");
                    }
                }

                try
                {
                    var folder = "product";
                    var imageURL = await _cloudflareStorageService.UploadImageAsync(updateProductDto.Image, folder);
                    updateProductDto.ImageURL = imageURL;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("Image", $"Yeni görsel yüklenirken hata oluştu: {ex.Message}");
                    return View(updateProductDto);
                }
            }

            var statusChanged = entity.Status != updateProductDto.Status;
            var priceChanged = entity.Price != updateProductDto.Price;
            var categoryChanged = entity.CategoryId != updateProductDto.CategoryId;
            var update = _mapper.Map(updateProductDto, entity);
            update.UpdateAt = DateTime.UtcNow;

            try
            {
                await _productService.UpdateAsync(update);
                TempData["SuccessMessage"] = $"\"{update.ProductName}\" ürünü başarıyla güncellendi.";

                if (statusChanged && update.DeletedAt == null)
                {
                    var totalProductsCount = await _productService.CountAsync(predicate: n => n.DeletedAt == null && n.Status == true);
                    await _hubContext.Clients.All.SendAsync("TotalProducts", totalProductsCount);
                }

                if (categoryChanged)
                {
                    var categoriesList = await _productService.CategoryWithProducts();
                    await _hubContext.Clients.All.SendAsync("CategoriesList", categoriesList);
                }

                if (statusChanged || priceChanged)
                    await _hubContext.Clients.All.SendAsync("ProductStatisticsChanged");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ürün güncellenirken veritabanı hatası oluştu: {ex.Message}");
                return View(updateProductDto);
            }
        }

    }
}
