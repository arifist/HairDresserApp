using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BarberBookingApp.Models;

public class Customer
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public List<Appointment> Appointments { get; set; } = new();
}
