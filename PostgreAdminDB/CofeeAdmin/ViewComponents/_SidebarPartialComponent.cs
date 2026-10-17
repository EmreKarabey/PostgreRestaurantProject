using System;
using System.Threading.Tasks;
using BusinessLayer.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace CofeeAdmin.ViewComponents
{
    public class _SidebarPartialComponent : ViewComponent
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IReservationService _reservationService;

        public _SidebarPartialComponent(ICategoryService categoryService, IProductService productService, IReservationService reservationService)
        {
            _categoryService = categoryService;
            _productService = productService;
            _reservationService = reservationService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var categories = await _categoryService.GetListAsync();
                ViewBag.CategoryCount = categories?.TotalItemCount ?? 0;
            }
            catch
            {
                ViewBag.CategoryCount = 0;
            }

            try
            {
                var products = await _productService.GetListAsync();
                ViewBag.ProductCount = products?.TotalItemCount ?? 0;
            }
            catch
            {
                ViewBag.ProductCount = 0;
            }

            try
            {
                var reservations = await _reservationService.GetListAsync(predicate: x => x.DeletedAt == null);
                ViewBag.ReservationCount = reservations?.TotalItemCount ?? 0;
            }
            catch
            {
                ViewBag.ReservationCount = 0;
            }

            return View();
        }
    }
}
