using System.ComponentModel.DataAnnotations;

namespace BarberBookingApp.Models;

public class Admin
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(100)]
    public string DisplayName { get; set; } = "Arif";
}
