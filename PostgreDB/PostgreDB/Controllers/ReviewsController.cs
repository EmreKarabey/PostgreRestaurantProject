using Application.Features.Reviews.Queries.GetActiveReviewsList;
using Microsoft.AspNetCore.Mvc;

namespace PostgreDB.Controllers
{
    public class ReviewsController : BaseController
    {

        public async Task<IActionResult> Index()
        {
            var reviews = await Mediator.Send(new GetActiveReviewsListQuery());
            return View(reviews);
        }
    }
}
