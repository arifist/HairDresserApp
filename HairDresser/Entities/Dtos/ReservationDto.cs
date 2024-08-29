using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Entities.Dtos
{
    public record ReservationDto
    {
        public int ReservationId { get; init; }
        public DateTime ReservationDay { get; set; }
        public String HairCutTypes { get; set; }
        public DateTime ReservationHour { get; set; }
        public string? ReservationMessage { get; set; }
        public string ReservationName { get; set; }
        public string? ServiceType { get; init; }
        public DateTime? Date { get; set; } = DateTime.Now;
        //public DateTime WorkStartTime { get; set; }
        //public DateTime WorkEndTime { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime ReservationDate
		{
			get
			{
				return ReservationDay.Date.Add(ReservationHour.TimeOfDay);
			}
		}
	}
}
