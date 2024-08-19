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
        public IActionResult Index()
        {
            TempData["info"] = "Please fill the form.";
            return View();
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Index([FromForm] ReservationDtoForInsertion reservationDto)
        //{
        //	if (ModelState.IsValid)
        //	{


        //		_manager.ReservationService.CreateReservation(reservationDto);
        //		TempData["success"] = $"{reservationDto.ReservationId} has been created.";
        //		return RedirectToAction("Index");
        //	}
        //	return View();
        //}


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Index([FromForm] ReservationDtoForInsertion reservationDto)
        //{
        //	if (ModelState.IsValid)
        //	{
        //		// Seçilen zaman diliminde başka bir rezervasyon var mı kontrol ediyoruz
        //		if (!_manager.ReservationService.IsReservationSlotAvailable(reservationDto.ReservationDate))
        //		{
        //			TempData["error"] = "Bu saatte zaten bir rezervasyon mevcut. Lütfen başka bir saat seçin.";
        //			return View(reservationDto);
        //		}

        //		// Rezervasyon mevcut değilse, yeni rezervasyonu kaydediyoruz
        //		_manager.ReservationService.CreateReservation(reservationDto);
        //		TempData["success"] = $"{reservationDto.ReservationId} rezervasyonunuz oluşturuldu.";
        //		return RedirectToAction("Index");
        //	}
        //	return View();
        //}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([FromForm] ReservationDtoForInsertion reservationDto)
        {
            if (ModelState.IsValid)
            {
                // Seçilen zaman diliminde başka bir rezervasyon var mı kontrol ediyoruz
                if (!_manager.ReservationService.IsReservationSlotAvailable(reservationDto.ReservationDate))
                {
                    return Json(new { success = false, message = "Bu saatte zaten bir rezervasyon mevcut. Lütfen başka bir saat seçin." });
                }

                // Rezervasyon mevcut değilse, yeni rezervasyonu kaydediyoruz
                _manager.ReservationService.CreateReservation(reservationDto);
                return Json(new { success = true, message = $"{reservationDto.ReservationId} rezervasyonunuz oluşturuldu." });
            }

            return Json(new { success = false, message = "Geçersiz form verileri." });
        }



    }
}

