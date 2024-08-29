using Entities.Dtos;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Services.Contracts;

namespace HairDresserApp.Controllers
{

    public class ReservationController : Controller
    {
        private readonly IServiceManager _manager;

        public ReservationController(IServiceManager manager)
        {
            _manager = manager;
        }



        public async Task<IActionResult> Index()
        {


            var workTime = await _manager.WorkTimeService.GetWorkTimeAsync(1);
            ViewBag.WorkStartTime = workTime.WorkStartTime.ToString("HH:mm");
            ViewBag.WorkEndTime = workTime.WorkEndTime.ToString("HH:mm");
            TempData["info"] = "Please fill the form.";
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([FromForm] ReservationDtoForInsertion reservationDto)
        {


            if (ModelState.IsValid)
            {
                if (!_manager.ReservationService.IsReservationSlotAvailable(reservationDto.ReservationDate, reservationDto.HairCutTypes))
                {
                    return Json(new { success = false, message = "Bu saatte veya 30 dakika sonrası için uygun olmayan bir rezervasyon mevcut. Lütfen başka bir saat seçin." });
                }

                _manager.ReservationService.CreateReservation(reservationDto);
                return Json(new { success = true, message = $"{reservationDto.ReservationId} rezervasyonunuz oluşturuldu." });
            }

            return Json(new { success = false, message = "Geçersiz form verileri." });
        }



        [HttpGet]
        public async Task<IActionResult> GetOccupiedHours(DateTime date)
        {
            var reservations = _manager.ReservationService.GetReservationsByDay(date, false);

            var occupiedHours = reservations
                .Select(r => r.ReservationDate.ToString("HH:mm")) // Saat kısmını al
                .Distinct()
                .ToList();

            return Json(occupiedHours); // JSON olarak döndür
        }



    }
}

