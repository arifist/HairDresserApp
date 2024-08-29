﻿using Entities.Dtos;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.Contracts;
using System.Security.Claims;

namespace HairDresserApp.Controllers
{

    public class ReservationController : Controller
    {
        private readonly IServiceManager _manager;
        private readonly UserManager<IdentityUser> _userManager; // UserManager servisi
        private readonly SignInManager<IdentityUser> _signInManager; // Giriş işlemleri için SignInManager servisi

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



        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Index([FromForm] ReservationDtoForInsertion reservationDto)
        //{


        //    if (ModelState.IsValid)
        //    {
        //        if (!_manager.ReservationService.IsReservationSlotAvailable(reservationDto.ReservationDate, reservationDto.HairCutTypes))
        //        {
        //            return Json(new { success = false, message = "Bu saatte veya 30 dakika sonrası için uygun olmayan bir rezervasyon mevcut. Lütfen başka bir saat seçin." });
        //        }

        //        _manager.ReservationService.CreateReservation(reservationDto);
        //        return Json(new { success = true, message = $"{reservationDto.ReservationId} rezervasyonunuz oluşturuldu." });
        //    }

        //    return Json(new { success = false, message = "Geçersiz form verileri." });
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Index([FromForm] ReservationDtoForInsertion reservationDto)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        // Kullanıcı bilgilerini al
        //        reservationDto.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Kullanıcı ID
        //        reservationDto.UserName = User.Identity?.Name; // Kullanıcı Adı

        //        if (!_manager.ReservationService.IsReservationSlotAvailable(reservationDto.ReservationDate, reservationDto.HairCutTypes))
        //        {
        //            return Json(new { success = false, message = "Bu saatte veya 30 dakika sonrası için uygun olmayan bir rezervasyon mevcut. Lütfen başka bir saat seçin." });
        //        }

        //        _manager.ReservationService.CreateReservation(reservationDto);
        //        return Json(new { success = true, message = $"{reservationDto.ReservationId} rezervasyonunuz oluşturuldu." });
        //    }

        //    return Json(new { success = false, message = "Geçersiz form verileri." });
        //}



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([FromForm] ReservationDtoForInsertion reservationDto)
        {
            // Kullanıcının giriş yapıp yapmadığını kontrol et
            if (!User.Identity.IsAuthenticated)
            {
                // Giriş yapılmadıysa mesaj göster ve rezervasyonu engelle
                return Json(new { success = false, message = "Lütfen giriş yapın." });
            }

            if (ModelState.IsValid)
            {
                // Kullanıcı bilgilerini al
                reservationDto.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Kullanıcı ID
                reservationDto.UserName = User.Identity?.Name; // Kullanıcı Adı

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

