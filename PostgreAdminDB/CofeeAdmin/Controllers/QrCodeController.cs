using BusinessLayer.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CofeeAdmin.Controllers
{

    [Authorize(Roles = "Admin")]
    public class QrCodeController : Controller
    {
        private readonly IQrCodeService _qrCodeService;

        public QrCodeController(IQrCodeService qrCodeService)
        {
            _qrCodeService = qrCodeService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(string value, int tableNumber)
        {
            if (string.IsNullOrWhiteSpace(value) || tableNumber < 1)
            {
                ModelState.AddModelError(string.Empty, "Geçerli bir adres ve masa seçin.");
                return View();
            }

            ViewBag.QrCode = _qrCodeService.GenerateQrCode(value);
            ViewBag.QrValue = value;
            ViewBag.TableNumber = tableNumber;
            return View();
        }
    }
}
