using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public record ReservationDto
    {
        public int ReservationId { get; init; }
        public int? CustomerId { get; init; }
        public Customer? Customer { get; init; }
        public int? HairdresserId { get; init; }
        public HairDresser? Hairdresser { get; init; }
        public DateTime? ReservationDate { get; set; }
        public string? ServiceType { get; init; }
        public DateTime? Date { get; set; }
    }
}
