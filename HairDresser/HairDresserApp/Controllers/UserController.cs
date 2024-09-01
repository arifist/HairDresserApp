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
    }
}
