using System.Threading.Tasks;
using BusinessLayer.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace CofeeAdmin.ViewComponents
{
    public class _DashboardMenuListComponent:ViewComponent
    {
        private readonly IReservationService _reservationService;

        public _DashboardMenuListComponent(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var list = await _reservationService.GetListAsync(pageSize:10,page:1);
            return View(list);
        }
    }
}
