using System.Linq;
using System.Threading.Tasks;
using BusinessLayer.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace CofeeAdmin.ViewComponents
{
    public class RestaurantTableQrSelectorViewComponent : ViewComponent
    {
        private readonly IRestaurantTableService _restaurantTableService;

        public RestaurantTableQrSelectorViewComponent(IRestaurantTableService restaurantTableService)
        {
            _restaurantTableService = restaurantTableService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? selectedTable = null)
        {
            var tables = await _restaurantTableService.GetListAsync(
                predicate: table => table.DeletedAt == null,
                orderBy: query => query.OrderBy(table => table.TableNumber));

            ViewBag.SelectedTable = selectedTable ?? tables.FirstOrDefault()?.TableNumber;
            return View(tables.ToList());
        }
    }
}
