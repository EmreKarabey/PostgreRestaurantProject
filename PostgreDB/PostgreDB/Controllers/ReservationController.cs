using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Features.Reservation.Command.Create;
using Application.Features.Reservation.Queries.GetMyReservatioLs;
using Application.Features.Reservation.Queries.GetTableActive;
using Application.Features.RestaurantTable.Queries.GetActiveTableList;
using Application.Services.Cache;
using Domain.Entities;
using Domain.Packages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PostgreDB.Dto;
using PostgreDB.SignalR;

namespace PostgreDB.Controllers
{
    public class ReservationController : BaseController
    {
        private readonly ICacheService _cacheService;
        private readonly IHubContext<SignalRHub> _hubContext;

        public ReservationController(ICacheService cacheService, IHubContext<SignalRHub> hubContext)
        {
            _cacheService = cacheService;
            _hubContext = hubContext;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index([FromQuery] string? tableNumber)
        {
            TempData["TableNumber"] = !string.IsNullOrEmpty(tableNumber) ? tableNumber : null;

            var tables = await Mediator.Send(new GetActiveTableListQuery());

            return View(tables);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize]
        public async Task<IActionResult> Index(CreateReservationDto createReservationDto)
        {
            var qrTableNumber = int.TryParse(TempData["TableNumber"]?.ToString(), out var parsedTableNumber)
                ? parsedTableNumber
                : 0;
            var tableNumber = createReservationDto.TableNumber ?? qrTableNumber;

            if (tableNumber > 0)
            {
                var any = await Mediator.Send(new GetTableActiveQuery
                {
                    ReservationDate = createReservationDto.ReservationDate.ToUniversalTime(),
                    TableNo = tableNumber
                });

                if (any)
                {
                    TempData["ErrorMessage"] = "Bu masa seçtiğiniz tarih ve saatte dolu. Lütfen başka bir tarih/saat seçin ya da krokiden başka bir masa seçin.";
                    var tables = await Mediator.Send(new GetActiveTableListQuery());
                    return View(tables);
                }
            }

            await Mediator.Send(new CreateReservationCommand
            {
                FirstName = createReservationDto.FirstName,
                LastName = createReservationDto.LastName,
                Email = createReservationDto.Email,
                ReservationDate = createReservationDto.ReservationDate.ToUniversalTime(),
                NumberOfPeople = createReservationDto.NumberOfPeople,
                PhoneNumber = createReservationDto.PhoneNumber,
                TableNumber = tableNumber
            });

            await SignalRHub.SendActiveTablesAsync(_hubContext, Mediator);

            TempData["SuccessMessage"] = "Rezervasyonunuz başarıyla kaydedildi.";
            return RedirectToAction("Index");
        }



        [HttpGet]
        public async Task<IActionResult> MyReservations(int size = 15, int page = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (User.Identity?.IsAuthenticated != true || !int.TryParse(userId, out _))
            {
                Response.Cookies.Delete("AccessToken");
                return RedirectToAction("Index", "Login");
            }

            var paged = new PagedResult(size, page);
            var list = await Mediator.Send(new GetMyRservationListQuery { pagedResult = paged });
            return View(list);
        }

        [HttpGet]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> TriggerTableRefresh()
        {
            await SignalRHub.SendActiveTablesAsync(_hubContext, Mediator);
            return Ok(new { success = true });
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize]
        public async Task<IActionResult> RegisterCache(int TableNumber, DateTime ReservationDate)
        {
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            var userId = HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

            var cacheKey = $"TableLock_{TableNumber}_{ReservationDate.ToUniversalTime():yyyyMMddHHmm}";

            var lockOwner = await _cacheService.GetAsync<string>(cacheKey);
            if (lockOwner != null && lockOwner != userId)
            {
                return Json(new { success = false, message = "Bu masa üzerinde şu anda başka bir işlem yapılıyor." });
            }

            await _cacheService.SetAsync(
                key: cacheKey,
                value: userId,
                expiration: TimeSpan.FromMinutes(5)
            );

            if (isAjax)
                return Json(new { success = true, message = "Başarılı bir şekilde masa seçildi" });

            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize]
        public async Task<IActionResult> ReleaseCache(int TableNumber, DateTime ReservationDate)
        {
            var cacheKey = $"TableLock_{TableNumber}_{ReservationDate.ToUniversalTime():yyyyMMddHHmm}";
            var lockOwner = await _cacheService.GetAsync<string>(cacheKey);
            var userId = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;

            if (lockOwner != null && lockOwner == userId)
            {
                await _cacheService.RemoveAsync(cacheKey);
            }

            return Ok(new { success = true });
        }


    }
}
