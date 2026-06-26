using BarberBookingApp.Models;

namespace BarberBookingApp.Services;

public interface IAppointmentService
{
    Task<List<TimeSpan>> GetAvailableSlotsAsync(DateOnly date, int serviceTypeId);

    Task<AppointmentResult> CreateAppointmentAsync(int customerId, int serviceTypeId, DateTime startTime);

    Task<List<Appointment>> GetCustomerAppointmentsAsync(int customerId);

    Task<List<Appointment>> GetAllAppointmentsAsync(DateOnly? date = null, AppointmentStatus? status = null);

    Task<AppointmentResult> CancelAppointmentAsync(int appointmentId, string cancelledBy, string? reason);

    Task<AppointmentResult> CancelOwnAppointmentAsync(int appointmentId, int customerId);

    Task<AppointmentResult> CreateWalkInAppointmentAsync(string fullName, string phoneNumber, int serviceTypeId, DateTime startTime);

    DateOnly GetMinBookingDate();

    DateOnly GetMaxBookingDate();
}

public record AppointmentResult(bool Success, string? ErrorMessage, Appointment? Appointment = null);
