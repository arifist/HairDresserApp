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
        public IActionResult Create()
        {
            TempData["info"] = "Please fill the form.";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] ReservationDtoForInsertion reservationDto)
        {
            if (ModelState.IsValid)
            {

                _manager.ReservationService.CreateReservation(reservationDto);
                TempData["success"] = $"{reservationDto.ReservationId} has been created.";
                return RedirectToAction("Index");
            }
            return View();
        }

        //[HttpGet("available-timeslots")]
        //public IActionResult GetAvailableTimeSlots([FromQuery] DateTime date)
        //{
        //    var availableTimeSlots = _reservationService.GetAvailableTimeSlots(date);
        //    return Ok(availableTimeSlots);
        //}

        //[HttpPost]
        //public IActionResult CreateReservation([FromBody] Reservation reservation)
        //{
        //    _reservationService.CreateReservation(reservation);
        //    return Ok(reservation);
        //}

        public IActionResult Index()
        {
            return View();
        }

    }
}

