using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Contracts;

namespace HairDresserApp.Controllers
{
    [Authorize]
    public class UserController: Controller
    {
        private readonly IServiceManager _manager;

        public UserController(IServiceManager manager)
        {
            _manager = manager;
        }

        public async Task<IActionResult> Index()
        {
            // Kullanıcının kimlik bilgilerini alıyoruz
            var user = HttpContext.User;

            try
            {
                // Kullanıcının rezervasyonlarını çekiyoruz
                var reservations = await _manager.AuthService.GetReservationsByUserAsync(user);

                // Rezervasyonları view'a gönderiyoruz
                return View(reservations);
            }
            catch (Exception ex)
            {
                // Hata durumunda bir mesaj gösterebiliriz
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteReservation(int reservationId)
        {
            try
            {
                // Silme işlemini gerçekleştir
                bool result = await _manager.ReservationService.DeleteOneReservationByIdAsync(reservationId);
                if (!result)
                {
                    // Hata mesajı, rezervasyon bulunamadı
                    TempData["Error"] = "Rezervasyon bulunamadı veya silinemedi.";
                    return RedirectToAction("Index");
                }

                TempData["Success"] = "Rezervasyon başarıyla silindi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Hata durumunda error sayfası
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }
    }
}
