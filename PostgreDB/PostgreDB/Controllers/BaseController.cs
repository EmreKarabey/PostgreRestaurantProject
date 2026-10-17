using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace PostgreDB.Controllers
{
    public class BaseController : Controller
    {
        private IMediator? _mediator;

        protected IMediator? Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();
    }
}
