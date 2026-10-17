using System.Threading.Tasks;
using Application.Features.Category.Queries.GetActiveList;
using Application.Features.Product.Queries.GetActiveWithProductsList;
using Microsoft.AspNetCore.Mvc;

namespace PostgreDB.Controllers
{
    public class MenuController : BaseController
    {

        public async Task<IActionResult> Index()
        {
            var list = await Mediator.Send(new GetActiveWithProductsListQuery());
            return View(list);
        }
    }
}
