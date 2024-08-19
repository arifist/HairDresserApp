using HairDresserApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Contracts;

namespace HairDresserApp.Admin.Controllers
{
    [Area("Admin")]
    public class ReservationsController : Controller
    {
        private readonly IServiceManager _manager;
        public ReservationsController(IServiceManager manager)
        {
            _manager = manager;
        }
        public IActionResult Index()
        {
            var reservations = _manager.ReservationService.GetAllReservations(true);
            return View(new ReservationListViewModel()
            {
                Reservations = reservations,
            });
        }
        public IActionResult Get([FromRoute(Name = "id")] int id)
        {
            var model = _manager.ReservationService.GetOneReservation(id, false);
            ViewData["Title"] = model?.ReservationName;
            return View(model);
        }
    }
}
