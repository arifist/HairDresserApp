using Entities.Dtos;
using Entities.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.Contracts;
using Services;
using Services.Contracts;


namespace HairDresserApp.Infrastructure.Extensions
{
    public static class ServiceExtension
    {
        public static void ConfigureDbContext(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<RepositoryContext>(options =>
            {
                options.UseSqlite(configuration.GetConnectionString("sqlconnection"),
                    b => b.MigrationsAssembly("HairDresserApp"));

                options.EnableSensitiveDataLogging(true);
            });
        }

        public static void ConfigureIdentity(this IServiceCollection services)
        {
            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.User.RequireUniqueEmail = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                // �ki fakt�rl� do�rulama i�in SMS token sa�lay�c�s�n� kullanma
                options.Tokens.ProviderMap.Add("Phone", new TokenProviderDescriptor(typeof(Microsoft.AspNetCore.Identity.PhoneNumberTokenProvider<AppUser>)));
            })
            .AddEntityFrameworkStores<RepositoryContext>()
            .AddDefaultTokenProviders(); // Varsay�lan token sa�lay�c�lar� ekler

            // SMS do�rulama sa�lay�c�s�n� kaydedin
            services.AddTransient<IUserTwoFactorTokenProvider<AppUser>, Microsoft.AspNetCore.Identity.PhoneNumberTokenProvider<AppUser>>();
        }


        public static void ConfigureSession(this IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.Name = "HairDresserApp.Session";
                options.IdleTimeout = TimeSpan.FromMinutes(10);
            });
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        public static void ConfigureRepositoryRegistration(this IServiceCollection services)
        {
            services.AddScoped<IRepositoryManager, RepositoryManager>();
            services.AddScoped<IReservationRepository, ReservationRepository>();
            services.AddScoped<IWorkTimeRepository, WorkTimeRepository>();


        }

        public static void ConfigureServiceRegistration(this IServiceCollection services)
        {
            services.AddScoped<IServiceManager, ServiceManager>();
            services.AddScoped<IReservationService, ReservationManager>();
            services.AddScoped<IAuthService, AuthManager>();
            services.AddScoped<IWorkTimeService, WorkTimeManager>();
            services.AddScoped<IDeletePastReservationsService, DeletePastReservationsService>();
            services.AddScoped<ISMSSenderService, SMSSenderService>();

        }

        public static void ConfigureApplicationCookie(this IServiceCollection services)
        {
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = new PathString("/Account/Login");
                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
                options.AccessDeniedPath = new PathString("/Account/AccessDenied");
            });
        }

        public static void ConfigureRouting(this IServiceCollection services)
        {
            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
                options.AppendTrailingSlash = false;
            });
        }


    }
}