using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLayer.Abstract;
using CofeeAdmin.Dto.Reservation;
using CofeeAdmin.Services.Sync;
using CofeeAdmin.SignalR;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CofeeAdmin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReservationController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly IMapper _mapper;
        private readonly IClientSyncService _clientSyncService;
        private readonly IHubContext<SignalRHub> _hubContext;
        private readonly UserManager<AppUser> _userManager;

        public ReservationController(IReservationService reservationService, IMapper mapper, IClientSyncService clientSyncService, IHubContext<SignalRHub> hubContext, UserManager<AppUser> userManager)
        {
            _reservationService = reservationService;
            _mapper = mapper;
            _clientSyncService = clientSyncService;
            _hubContext = hubContext;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int size = 15, int page = 1, string? filter = null)
        {
            var allReservations = await _reservationService.GetListAsync();
            var allList = allReservations?.ToList() ?? new System.Collections.Generic.List<Reservation>();

            ViewBag.TotalReservations = allList.Count;
            ViewBag.ActiveReservations = allList.Count(x => x.DeletedAt == null);
            ViewBag.SoftDeletedReservations = allList.Count(x => x.DeletedAt != null);
            ViewBag.UpcomingReservations = allList.Count(x => x.DeletedAt == null && x.ReservationDate >= DateTime.UtcNow);
            ViewBag.TotalPeople = allList.Where(x => x.DeletedAt == null).Sum(x => x.NumberOfPeople);
            ViewBag.CurrentFilter = filter ?? "all";

            Expression<Func<Reservation, bool>>? predicate = null;
            if (filter == "active")
            {
                predicate = x => x.DeletedAt == null;
            }
            else if (filter == "deleted")
            {
                predicate = x => x.DeletedAt != null;
            }

            var list = await _reservationService.GetListAsync(
                predicate: predicate,
                orderBy: q => q.OrderByDescending(x => x.ReservationDate),
                page: page,
                pageSize: size
            );

            var normalizedEmails = list
                .Where(reservation => !string.IsNullOrWhiteSpace(reservation.Email))
                .Select(reservation => _userManager.NormalizeEmail(reservation.Email))
                .Distinct()
                .ToList();

            var profileImages = await _userManager.Users
                .Where(user => user.NormalizedEmail != null && normalizedEmails.Contains(user.NormalizedEmail))
                .Select(user => new { user.NormalizedEmail, user.ProfileImageURL })
                .ToListAsync();

            ViewBag.ReservationProfileImages = profileImages
                .Where(user => !string.IsNullOrWhiteSpace(user.ProfileImageURL))
                .ToDictionary(user => user.NormalizedEmail!, user => user.ProfileImageURL!);

            return View(list);
        }

        [HttpGet]
        public IActionResult AddReservation()
        {
            return View(new AddReservationDto());
        }

        [HttpPost]
        public async Task<IActionResult> AddReservation([FromForm] AddReservationDto addReservationDto)
        {
            if (!ModelState.IsValid)
            {
                return View(addReservationDto);
            }

            var entity = _mapper.Map<Reservation>(addReservationDto);
            entity.ReservationDate = DateTime.SpecifyKind(addReservationDto.ReservationDate, DateTimeKind.Utc);
            entity.CreatedAt = DateTime.UtcNow;

            try
            {
                await _reservationService.AddAsync(entity);
                await _clientSyncService.NotifyTableRefreshAsync();
                TempData["SuccessMessage"] = $"\"{entity.FirstName} {entity.LastName}\" adına rezervasyon başarıyla oluşturuldu.";

                var resrevationCount = await _reservationService.CountAsync(predicate: n => n.DeletedAt == null);
                await _hubContext.Clients.All.SendAsync("ReservationCount", resrevationCount);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Rezervasyon kaydedilirken hata oluştu: {ex.Message}");
                return View(addReservationDto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> UpdateReservation(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Geçersiz rezervasyon ID'si.";
                return RedirectToAction("Index");
            }

            var entity = await _reservationService.GetByIdAsync(id);
            if (entity == null)
            {
                TempData["ErrorMessage"] = "Güncellemek istediğiniz rezervasyon bulunamadı.";
                return RedirectToAction("Index");
            }

            var dto = _mapper.Map<UpdateReservationDto>(entity);
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateReservation([FromForm] UpdateReservationDto updateReservationDto)
        {
            if (!ModelState.IsValid)
            {
                return View(updateReservationDto);
            }

            var entity = await _reservationService.GetByIdAsync(updateReservationDto.EntityID);
            if (entity == null)
            {
                TempData["ErrorMessage"] = "Güncellenecek rezervasyon bulunamadı.";
                return RedirectToAction("Index");
            }

            entity.FirstName = updateReservationDto.FirstName;
            entity.LastName = updateReservationDto.LastName;
            entity.Email = updateReservationDto.Email;
            entity.PhoneNumber = updateReservationDto.PhoneNumber;
            entity.ReservationDate = DateTime.SpecifyKind(updateReservationDto.ReservationDate, DateTimeKind.Utc);
            entity.NumberOfPeople = updateReservationDto.NumberOfPeople;
            entity.TableNumber = updateReservationDto.TableNumber;
            entity.UpdateAt = DateTime.UtcNow;

            if (updateReservationDto.DeletedAt.HasValue)
            {
                entity.DeletedAt = DateTime.SpecifyKind(updateReservationDto.DeletedAt.Value, DateTimeKind.Utc);
            }
            else
            {
                entity.DeletedAt = null;
            }

            try
            {
                await _reservationService.UpdateAsync(entity);
                await _clientSyncService.NotifyTableRefreshAsync();
                TempData["SuccessMessage"] = $"\"{entity.FirstName} {entity.LastName}\" rezervasyonu başarıyla güncellendi.";

                var resrevationCount = await _reservationService.CountAsync(predicate: n => n.DeletedAt == null);
                await _hubContext.Clients.All.SendAsync("ReservationCount", resrevationCount);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Güncelleme sırasında hata oluştu: {ex.Message}");
                return View(updateReservationDto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleSoftDelete(int id, string? returnURL = null)
        {
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (id <= 0)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Geçersiz rezervasyon ID'si." });

                TempData["ErrorMessage"] = "Geçersiz rezervasyon ID'si.";
                return RedirectToAction("Index");
            }

            var entity = await _reservationService.GetByIdAsync(id);
            if (entity == null)
            {
                if (isAjax)
                    return Json(new { success = false, message = "İşlem yapılmak istenen rezervasyon bulunamadı." });

                TempData["ErrorMessage"] = "Rezervasyon bulunamadı.";
                return RedirectToAction("Index");
            }

            string successMsg;
            bool isDeleted;

            if (entity.DeletedAt == null)
            {
                entity.DeletedAt = DateTime.UtcNow;
                isDeleted = true;
                successMsg = $"\"{entity.FirstName} {entity.LastName}\" rezervasyonuna soft delete uygulandı (arşive alındı).";
            }
            else
            {
                entity.DeletedAt = null;
                isDeleted = false;
                successMsg = $"\"{entity.FirstName} {entity.LastName}\" rezervasyonu geri yüklendi ve aktif edildi.";
            }

            try
            {
                await _reservationService.UpdateAsync(entity);
                await _clientSyncService.NotifyTableRefreshAsync();

                var resrevationCount = await _reservationService.CountAsync(predicate: n => n.DeletedAt == null);
                await _hubContext.Clients.All.SendAsync("ReservationCount", resrevationCount);

                TempData["SuccessMessage"] = successMsg;

                if (isAjax)
                {
                    return Json(new
                    {
                        success = true,
                        message = successMsg,
                        isDeleted = isDeleted,
                        deletedAtText = entity.DeletedAt?.ToString("dd.MM.yyyy HH:mm") ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                if (isAjax)
                    return Json(new { success = false, message = $"İşlem sırasında hata: {ex.Message}" });

                TempData["ErrorMessage"] = $"İşlem sırasında hata: {ex.Message}";
            }

            if (!string.IsNullOrEmpty(returnURL) && Url.IsLocalUrl(returnURL))
            {
                return Redirect(returnURL);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, string? returnURL = null)
        {
            return await ToggleSoftDelete(id, returnURL);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReservation(int id, bool permanent = false, string? returnURL = null)
        {
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (id <= 0)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Geçersiz rezervasyon ID'si." });

                TempData["ErrorMessage"] = "Geçersiz rezervasyon ID'si.";
                return RedirectToAction("Index");
            }

            var entity = await _reservationService.GetByIdAsync(id);
            if (entity == null)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Silinmek istenen rezervasyon bulunamadı." });

                TempData["ErrorMessage"] = "Silinmek istenen rezervasyon bulunamadı.";
                return RedirectToAction("Index");
            }

            try
            {
                string msg;
                if (permanent)
                {
                    await _reservationService.DeleteAsync(entity);
                    msg = $"\"{entity.FirstName} {entity.LastName}\" rezervasyonu kalıcı olarak silindi.";
                }
                else
                {
                    entity.DeletedAt = DateTime.UtcNow;
                    await _reservationService.UpdateAsync(entity);
                    msg = $"\"{entity.FirstName} {entity.LastName}\" rezervasyonu arşive taşındı (soft delete).";
                }

                await _clientSyncService.NotifyTableRefreshAsync();

                TempData["SuccessMessage"] = msg;

                var resrevationCount = await _reservationService.CountAsync(predicate: n => n.DeletedAt == null);
                await _hubContext.Clients.All.SendAsync("ReservationCount", resrevationCount);

                if (isAjax)
                    return Json(new { success = true, message = msg, permanent = permanent });
            }
            catch (Exception ex)
            {
                if (isAjax)
                    return Json(new { success = false, message = $"Silme sırasında hata oluştu: {ex.Message}" });

                TempData["ErrorMessage"] = $"Silme sırasında hata oluştu: {ex.Message}";
            }

            if (!string.IsNullOrEmpty(returnURL) && Url.IsLocalUrl(returnURL))
            {
                return Redirect(returnURL);
            }

            return RedirectToAction("Index");
        }
    }
}
