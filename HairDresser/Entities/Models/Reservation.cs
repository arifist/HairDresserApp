namespace Entities.Models;

public class Reservation
{
    public int ReservationId { get; set; }
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    //public int? HairdresserId { get; set; }
    //public HairDresser? Hairdresser { get; set; }
    public DateTime ReservationDay { get; set; }
    public DateTime ReservationHour { get; set; }

    public DateTime ReservationDate { get; set; }

    //public string? ServiceType { get; set; }
    public string? ReservationMessage { get; set; }
    public string ReservationName { get; set; }
    public String HairCutTypes { get; set; }
    public DateTime? Date { get; set; } = DateTime.Now;
}