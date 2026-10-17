using System.Threading.Tasks;
using System.Linq;
using BusinessLayer.Abstract;
using CofeeAdmin.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CofeeAdmin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReviewsController : Controller
    {
        private readonly IReviewsService _reviewsService;
        private readonly IHubContext<SignalRHub> _hubContext;

        public ReviewsController(IReviewsService reviewsService, IHubContext<SignalRHub> hubContext)
        {
            _reviewsService = reviewsService;
            _hubContext = hubContext;
        }

        private Task NotifyReviewStatisticsChangedAsync()
        {
            return _hubContext.Clients.All.SendAsync("ReviewStatisticsChanged");
        }
        [HttpGet]
        public async Task<IActionResult> Index(string? search, int size = 15, int page = 1)
        {
            var searchText = search?.Trim();
            var searchPattern = string.IsNullOrWhiteSpace(searchText) ? null : $"%{searchText}%";
            var list = await _reviewsService.GetListAsync(
                predicate: searchPattern == null
                    ? null
                    : review => EF.Functions.ILike(review.FirstName, searchPattern)
                        || EF.Functions.ILike(review.LastName, searchPattern)
                        || (review.Product != null && EF.Functions.ILike(review.Product.ProductName, searchPattern))
                        || (review.Comment != null && EF.Functions.ILike(review.Comment, searchPattern)),
                include: query => query.Include(review => review.Product),
                orderBy: query => query.OrderByDescending(review => review.CreatedAt),
                pageSize: size,
                page: page);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                Response.Headers["X-Total-Count"] = list.TotalItemCount.ToString();
                Response.Headers["X-Page-Number"] = list.PageNumber.ToString();
                Response.Headers["X-Page-Size"] = list.PageSize.ToString();
                Response.Headers["X-Page-Count"] = list.PageCount.ToString();
                return PartialView("_ReviewRows", list);
            }

            return View(list);
        }


        [HttpPost]
        public async Task<IActionResult> HardDelete(int id)
        {
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (id < 0)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Geçersiz yorum ID'si." });

                ModelState.AddModelError("EntityID", "Id alanı boş.");
                return RedirectToAction("Index");
            }



            var entity = await _reviewsService.GetByIdAsync(id);

            if (entity == null)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Silmek istediğiniz yorum mevcut değil." });

                TempData["ErrorMessage"] = "Silmek istediğiniz yorum mevcut değil.";
                return RedirectToAction("Index");
            }
            try
            {
                await _reviewsService.DeleteAsync(entity);
                await NotifyReviewStatisticsChangedAsync();

                if (isAjax)
                    return Json(new { success = true, message = $"Yorum başarılı bir şekilde kalıcı silindi." });

                TempData["SuccessMessage"] = "Yorum başarılı bir şekilde silindi.";


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Yorum silinirken hata: {ex.Message}");
                if (isAjax)
                    return Json(new { success = false, message = $"Yorum silinirken hata oluştu: {ex.Message}" });
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (id < 0)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Geçersiz yorum ID'si." });

                ModelState.AddModelError("EntityID", "Id alanı boş.");
                return RedirectToAction("Index");
            }



            var entity = await _reviewsService.GetByIdAsync(id);

            if (entity == null)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Silmek istediğiniz yorum mevcut değil." });

                TempData["ErrorMessage"] = "Silmek istediğiniz yorum mevcut değil.";
                return RedirectToAction("Index");
            }
            try
            {
                entity.DeletedAt = DateTime.UtcNow;

                await _reviewsService.UpdateAsync(entity);
                await NotifyReviewStatisticsChangedAsync();

                if (isAjax)
                    return Json(new { success = true, message = $"Yorum başarılı bir şekilde silindi." });

                TempData["SuccessMessage"] = "Yorum başarılı bir şekilde silindi.";


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Yorum silinirken hata: {ex.Message}");
                if (isAjax)
                    return Json(new { success = false, message = $"Yorum silinirken hata oluştu: {ex.Message}" });
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Restore(int id)
        {
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (id < 0)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Geçersiz yorum ID'si." });

                ModelState.AddModelError("EntityID", "Id alanı boş.");
                return RedirectToAction("Index");
            }



            var entity = await _reviewsService.GetByIdAsync(id);

            if (entity == null)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Geri getirmek istediğiniz yorum mevcut değil." });

                TempData["ErrorMessage"] = "Geri getirmek istediğiniz yorum mevcut değil.";
                return RedirectToAction("Index");
            }
            try
            {
                entity.DeletedAt = null;

                await _reviewsService.UpdateAsync(entity);
                await NotifyReviewStatisticsChangedAsync();

                if (isAjax)
                    return Json(new { success = true, message = $"Yorum başarılı bir şekilde geri getirildi." });

                TempData["SuccessMessage"] = "Yorum başarılı bir şekilde geri getirildi.";


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Yorum geri getirilirken hata: {ex.Message}");
                if (isAjax)
                    return Json(new { success = false, message = $"Yorum geri getirilirken hata oluştu: {ex.Message}" });
            }

            return RedirectToAction("Index");
        }

    }
}
