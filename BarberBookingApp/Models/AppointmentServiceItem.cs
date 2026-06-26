using System.Text.Json.Serialization;

namespace BarberBookingApp.Models;

public class AppointmentServiceItem
{
    public int Id { get; set; }

    public int AppointmentId { get; set; }
    [JsonIgnore]
    public Appointment? Appointment { get; set; }

    public int ServiceTypeId { get; set; }
    [JsonIgnore]
    public ServiceType? ServiceType { get; set; }
}
