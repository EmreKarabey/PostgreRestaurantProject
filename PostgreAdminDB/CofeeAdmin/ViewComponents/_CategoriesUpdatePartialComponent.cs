using System.Threading.Tasks;
using BusinessLayer.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace CofeeAdmin.ViewComponents
{
    public class _CategoriesUpdatePartialComponent : ViewComponent
    {
        private readonly ICategoryService _categoryService;

        public _CategoriesUpdatePartialComponent(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? selectedCategoryId = null)
        {
            var list = await _categoryService.GetListAsync();
            ViewBag.SelectedCategoryId = selectedCategoryId;
            return View(list);
        }
    }
}
