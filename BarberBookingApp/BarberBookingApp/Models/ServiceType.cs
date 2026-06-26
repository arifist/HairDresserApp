using System.ComponentModel.DataAnnotations;

namespace BarberBookingApp.Models;

public class ServiceType
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Description { get; set; }

    [Range(1, 480)]
    public int DurationMinutes { get; set; }

    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    [MaxLength(50)]
    public string Icon { get; set; } = "bi-scissors";
}
