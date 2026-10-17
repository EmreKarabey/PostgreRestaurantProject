using System.Threading.Tasks;
using Application.Features.Login.Command.Login;
using Application.Features.Login.Command.Register;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PostgreDB.Dto;

namespace PostgreDB.Controllers
{
    public class LoginController : BaseController
    {
        private readonly IMapper _mapper;

        public LoginController(IMapper mapper)
        {
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new LoginCommand());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginCommand command)
        {
            var result = await Mediator!.Send(command);

            if (!result.Status)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(command);
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax
            };

            if (command.rememberMe)
            {
                cookieOptions.Expires = DateTimeOffset.UtcNow.AddDays(7);
            }

            Response.Cookies.Append("AccessToken", result.Token!, cookieOptions);
            TempData["LoginSuccess"] = result.Message;
            return RedirectToAction("MyReservations", "Reservation");
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return View(registerDto);
            }

            var mapperRegister = _mapper.Map<RegisterCommand>(registerDto);

            var result = await Mediator.Send(mapperRegister);

            if (result.Status)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
                return View(registerDto);
            }
        }


    }
}
