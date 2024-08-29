namespace Entities.Models;

public class Reservation
{
    public int ReservationId { get; set; }
    public DateTime ReservationDay { get; set; }
    public DateTime ReservationHour { get; set; }
    public DateTime ReservationDate { get; set; }
    public string? ReservationMessage { get; set; }
    public string ReservationName { get; set; }
    public String HairCutTypes { get; set; }
    public DateTime? Date { get; set; } = DateTime.Now;
    public string? UserId { get; set; }
    public string? UserName { get; set; }

}