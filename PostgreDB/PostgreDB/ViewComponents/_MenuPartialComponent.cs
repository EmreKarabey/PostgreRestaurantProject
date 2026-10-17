using System.Threading.Tasks;
using Application.Features.Product.Queries.GetActiveWithProductsList;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace PostgreDB.ViewComponents
{
    public class _MenuPartialComponent:ViewComponent
    {
        private readonly IMediator _mediator;

        public _MenuPartialComponent(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var list = await _mediator.Send(new GetActiveWithProductsListQuery());
            return View(list);
        }
    }
}
