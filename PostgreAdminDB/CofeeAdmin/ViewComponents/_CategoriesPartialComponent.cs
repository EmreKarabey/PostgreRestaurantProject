using System.Threading.Tasks;
using BusinessLayer.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace CofeeAdmin.ViewComponents
{
    public class _CategoriesPartialComponent:ViewComponent
    {
        private readonly ICategoryService _categoryService;

        public _CategoriesPartialComponent(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var list = await _categoryService.GetListAsync();
            return View(list);
        }
    }
}
