using BerberArif.Models;
using Microsoft.EntityFrameworkCore;

namespace BerberArif.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<ServiceType> ServiceTypes => Set<ServiceType>();
    public DbSet<WorkingHour> WorkingHours => Set<WorkingHour>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();
    public DbSet<SmsLog> SmsLogs => Set<SmsLog>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.PhoneNumber)
            .IsUnique();

        modelBuilder.Entity<Admin>()
            .HasIndex(a => a.Username)
            .IsUnique();

        modelBuilder.Entity<WorkingHour>()
            .HasIndex(w => w.DayOfWeek)
            .IsUnique();

        modelBuilder.Entity<ServiceType>()
            .Property(s => s.Price)
            .HasColumnType("decimal(10,2)");

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Customer)
            .WithMany(c => c.Appointments)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.ServiceType)
            .WithMany()
            .HasForeignKey(a => a.ServiceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasIndex(a => new { a.StartTime, a.Status });
    }
}
