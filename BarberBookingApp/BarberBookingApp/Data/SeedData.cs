using BarberBookingApp.Models;
using Microsoft.AspNetCore.Identity;

namespace BarberBookingApp.Data;

public static class SeedData
{
    public const string DefaultAdminUsername = "arif";
    public const string DefaultAdminPassword = "Arif2026!";

    public static void Initialize(AppDbContext db)
    {
        if (!db.Admins.Any())
        {
            var admin = new Admin
            {
                Username = DefaultAdminUsername,
                DisplayName = "Arif",
            };
            admin.PasswordHash = new PasswordHasher<Admin>().HashPassword(admin, DefaultAdminPassword);
            db.Admins.Add(admin);
        }

        if (!db.ServiceTypes.Any())
        {
            db.ServiceTypes.AddRange(
                new ServiceType
                {
                    Name = "Saç Kesimi",
                    Description = "Yüz şekline ve saç yapısına uygun, makas ve tıraş makinesi ile profesyonel saç kesimi.",
                    DurationMinutes = 30,
                    Price = 250,
                    SortOrder = 1,
                    Icon = "bi-scissors"
                },
                new ServiceType
                {
                    Name = "Sakal Tıraşı",
                    Description = "Sıcak havlu ve özel jilet bakımı ile şekillendirme dahil sakal tıraşı.",
                    DurationMinutes = 30,
                    Price = 200,
                    SortOrder = 2,
                    Icon = "bi-droplet"
                },
                new ServiceType
                {
                    Name = "Saç + Sakal",
                    Description = "Saç kesimi ve sakal tıraşının birlikte uygulandığı ekonomik bakım paketi.",
                    DurationMinutes = 60,
                    Price = 400,
                    SortOrder = 3,
                    Icon = "bi-stars"
                },
                new ServiceType
                {
                    Name = "Diğer",
                    Description = "Yukarıdaki seçeneklere uymayan kısa danışma/bakım talepleri için.",
                    DurationMinutes = 30,
                    Price = 0,
                    SortOrder = 4,
                    Icon = "bi-three-dots"
                }
            );
        }

        if (!db.WorkingHours.Any())
        {
            var days = Enum.GetValues<DayOfWeek>();
            foreach (var day in days)
            {
                db.WorkingHours.Add(new WorkingHour
                {
                    DayOfWeek = day,
                    IsOpen = day != DayOfWeek.Friday,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(19, 0, 0)
                });
            }

            db.SaveChanges();
        }

        // Mevcut (zaten canlıda seed edilmiş) veritabanlarında da iş günü politikasını
        // uygula: Cuma kapalı, Cumartesi ve Pazar açık. Saat aralıklarına dokunulmaz.
        var friday = db.WorkingHours.FirstOrDefault(w => w.DayOfWeek == DayOfWeek.Friday);
        if (friday is not null)
        {
            friday.IsOpen = false;
        }

        var saturday = db.WorkingHours.FirstOrDefault(w => w.DayOfWeek == DayOfWeek.Saturday);
        if (saturday is not null)
        {
            saturday.IsOpen = true;
        }

        var sunday = db.WorkingHours.FirstOrDefault(w => w.DayOfWeek == DayOfWeek.Sunday);
        if (sunday is not null)
        {
            sunday.IsOpen = true;
        }

        db.SaveChanges();
    }
}
