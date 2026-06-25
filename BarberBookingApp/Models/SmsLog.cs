namespace BarberBookingApp.Models;

public class SmsLog
{
    public int Id { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Provider { get; set; } = string.Empty;

    public bool Success { get; set; }

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public string? ErrorMessage { get; set; }
}
