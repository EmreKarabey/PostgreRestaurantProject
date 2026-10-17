using Microsoft.AspNetCore.Mvc;

namespace CofeeAdmin.Controllers
{
    public class ErrorController : Controller
    {
        [HttpGet]
        [Route("Error/AccessDenied")]
        [Route("Error/403")]
        public IActionResult AccessDenied()
        {
            Response.StatusCode = 403;
            return View();
        }

        [HttpGet]
        [Route("Error/PageNotFound")]
        [Route("Error/404")]
        public IActionResult PageNotFound()
        {
            Response.StatusCode = 404;
            return View();
        }
    }
}
