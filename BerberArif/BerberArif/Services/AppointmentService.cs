using BerberArif.Data;
using BerberArif.Models;
using Microsoft.EntityFrameworkCore;

namespace BerberArif.Services;

public class AppointmentService : IAppointmentService
{
    private readonly AppDbContext _db;
    private readonly ISmsService _smsService;
    private readonly int _maxBookingDaysAhead;
    private readonly int _slotStepMinutes;

    public AppointmentService(AppDbContext db, ISmsService smsService, IConfiguration configuration)
    {
        _db = db;
        _smsService = smsService;
        _maxBookingDaysAhead = configuration.GetValue("AppSettings:MaxBookingDaysAhead", 7);
        _slotStepMinutes = configuration.GetValue("AppSettings:SlotStepMinutes", 15);
    }

    public DateOnly GetMinBookingDate() => DateOnly.FromDateTime(DateTime.Now);

    public DateOnly GetMaxBookingDate() => DateOnly.FromDateTime(DateTime.Now).AddDays(_maxBookingDaysAhead);

    public async Task<List<TimeSpan>> GetAvailableSlotsAsync(DateOnly date, int serviceTypeId)
    {
        if (date < GetMinBookingDate() || date > GetMaxBookingDate())
        {
            return new List<TimeSpan>();
        }

        var serviceType = await _db.ServiceTypes.FindAsync(serviceTypeId);
        if (serviceType is null || !serviceType.IsActive)
        {
            return new List<TimeSpan>();
        }

        var workingHour = await _db.WorkingHours
            .FirstOrDefaultAsync(w => w.DayOfWeek == date.DayOfWeek);

        if (workingHour is null || !workingHour.IsOpen)
        {
            return new List<TimeSpan>();
        }

        var dayStart = date.ToDateTime(TimeOnly.MinValue);
        var dayEnd = date.ToDateTime(TimeOnly.MaxValue);

        var existingAppointments = await _db.Appointments
            .Where(a => a.Status != AppointmentStatus.Cancelled &&
                        a.StartTime >= dayStart && a.StartTime <= dayEnd)
            .Select(a => new { a.StartTime, a.EndTime })
            .ToListAsync();

        var duration = TimeSpan.FromMinutes(serviceType.DurationMinutes);
        var step = TimeSpan.FromMinutes(_slotStepMinutes);
        var now = DateTime.Now;

        var slots = new List<TimeSpan>();
        var candidate = date.ToDateTime(TimeOnly.FromTimeSpan(workingHour.StartTime));
        var closing = date.ToDateTime(TimeOnly.FromTimeSpan(workingHour.EndTime));

        while (candidate + duration <= closing)
        {
            var candidateEnd = candidate + duration;

            var isPast = date == GetMinBookingDate() && candidate <= now;
            var overlaps = existingAppointments.Any(a => candidate < a.EndTime && a.StartTime < candidateEnd);

            if (!isPast && !overlaps)
            {
                slots.Add(candidate.TimeOfDay);
            }

            candidate = candidate.Add(step);
        }

        return slots;
    }

    public async Task<AppointmentResult> CreateAppointmentAsync(int customerId, int serviceTypeId, DateTime startTime)
    {
        var date = DateOnly.FromDateTime(startTime);

        var availableSlots = await GetAvailableSlotsAsync(date, serviceTypeId);
        if (!availableSlots.Contains(startTime.TimeOfDay))
        {
            return new AppointmentResult(false, "Seçilen saat artık uygun değil, lütfen başka bir saat seçin.");
        }

        var serviceType = await _db.ServiceTypes.FindAsync(serviceTypeId);
        if (serviceType is null)
        {
            return new AppointmentResult(false, "Hizmet bulunamadı.");
        }

        var customer = await _db.Customers.FindAsync(customerId);
        if (customer is null)
        {
            return new AppointmentResult(false, "Müşteri bulunamadı.");
        }

        var appointment = new Appointment
        {
            CustomerId = customerId,
            ServiceTypeId = serviceTypeId,
            StartTime = startTime,
            EndTime = startTime.AddMinutes(serviceType.DurationMinutes),
            Status = AppointmentStatus.Confirmed,
            CreatedAt = DateTime.UtcNow
        };

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();

        await _smsService.SendAsync(customer.PhoneNumber,
            $"Randevunuz alindi: {startTime:dd.MM.yyyy HH:mm} - {serviceType.Name}. Kuaför Arif sizi bekliyor!");

        return new AppointmentResult(true, null, appointment);
    }

    public async Task<List<Appointment>> GetCustomerAppointmentsAsync(int customerId)
    {
        return await _db.Appointments
            .Include(a => a.ServiceType)
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<List<Appointment>> GetAllAppointmentsAsync(DateOnly? date = null, AppointmentStatus? status = null)
    {
        var query = _db.Appointments
            .Include(a => a.Customer)
            .Include(a => a.ServiceType)
            .AsQueryable();

        if (date.HasValue)
        {
            var start = date.Value.ToDateTime(TimeOnly.MinValue);
            var end = date.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(a => a.StartTime >= start && a.StartTime <= end);
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        return await query.OrderBy(a => a.StartTime).ToListAsync();
    }

    public async Task<AppointmentResult> CancelAppointmentAsync(int appointmentId, string cancelledBy, string? reason)
    {
        var appointment = await _db.Appointments
            .Include(a => a.Customer)
            .Include(a => a.ServiceType)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment is null)
        {
            return new AppointmentResult(false, "Randevu bulunamadı.");
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return new AppointmentResult(false, "Randevu zaten iptal edilmiş.");
        }

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancelledAt = DateTime.UtcNow;
        appointment.CancelReason = reason;
        appointment.CancelledBy = cancelledBy;

        await _db.SaveChangesAsync();

        if (appointment.Customer is not null)
        {
            await _smsService.SendAsync(appointment.Customer.PhoneNumber,
                $"Randevunuz iptal edilmistir: {appointment.StartTime:dd.MM.yyyy HH:mm} - {appointment.ServiceType?.Name}. " +
                "Bilgi için Kuaför Arif ile iletişime geçebilirsiniz.");
        }

        return new AppointmentResult(true, null, appointment);
    }
}
