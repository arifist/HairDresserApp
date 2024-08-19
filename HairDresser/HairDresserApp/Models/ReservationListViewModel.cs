using Entities.Models;
using Microsoft.AspNetCore.Mvc;

namespace HairDresserApp.Models
{
    public class ReservationListViewModel
    {
        public IEnumerable<Reservation> Reservations { get; set; } = Enumerable.Empty<Reservation>();
        public Reservation Pagination { get; set; } = new();
        public int TotalCount => Reservations.Count();
    }
}
