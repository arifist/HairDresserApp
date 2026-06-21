using BerberArif.Components;
using BerberArif.Data;
using BerberArif.Endpoints;
using BerberArif.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Twilio;

var builder = WebApplication.CreateBuilder(args);

// Render/Docker gibi container ortamlarinda PORT env degiskeni ile dinleme adresi atanir.
var containerPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(containerPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{containerPort}");
}

var twilioAccountSid = builder.Configuration["Twilio:AccountSid"];
var twilioAuthToken = builder.Configuration["Twilio:AuthToken"];
if (!string.IsNullOrWhiteSpace(twilioAccountSid) && !string.IsNullOrWhiteSpace(twilioAuthToken))
{
    TwilioClient.Init(twilioAccountSid, twilioAuthToken);
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Database:Provider "Sqlite" (varsayılan, sunucu kurulumu gerektirmeyen tek dosyalık veritabanı) veya
// "SqlServer" (LocalDB/SQL Server, üretim için ölçeklenebilir kurulum) olarak ayarlanabilir.
// MSSQL koduna hiç dokunulmadı; appsettings.json'da "Database:Provider": "SqlServer" yapıp
// ConnectionStrings:DefaultConnection'ı geçerli bir SQL Server'a işaret ettirerek tekrar etkinleştirilebilir.
var dbProvider = builder.Configuration["Database:Provider"] ?? "Sqlite";
var useSqlServer = string.Equals(dbProvider, "SqlServer", StringComparison.OrdinalIgnoreCase);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (useSqlServer)
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
    else
    {
        var sqliteFile = builder.Configuration["Database:SqliteFile"] ?? "App_Data/berberarif.db";
        var sqlitePath = Path.Combine(builder.Environment.ContentRootPath, sqliteFile);
        Directory.CreateDirectory(Path.GetDirectoryName(sqlitePath)!);
        options.UseSqlite($"Data Source={sqlitePath}");
    }
});

builder.Services.AddHttpClient();

builder.Services.AddScoped<ISmsService, NetgsmSmsService>();
builder.Services.AddScoped<IOtpService, TwilioOtpService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/giris";
        options.AccessDeniedPath = "/giris";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;

        options.Events.OnRedirectToLogin = context =>
        {
            // Admin alanına yetkisiz erişim admin girişine, diğerleri müşteri girişine yönlendirilir.
            var isAdminRoute = context.Request.Path.StartsWithSegments("/admin");
            var loginPath = isAdminRoute ? "/admin/giris" : "/giris";
            var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
            context.Response.Redirect($"{loginPath}?returnUrl={returnUrl}");
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
});

builder.Services.AddCors();

// Render/Docker gibi reverse-proxy arkasinda calisirken X-Forwarded-Proto basligina
// guvenmezsek UseHttpsRedirection sonsuz yonlendirme dongusune girer.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (useSqlServer)
    {
        db.Database.Migrate();
    }
    else
    {
        // Sqlite modu: tek dosyalık veritabanı için migration geçmişi tutmuyoruz, şema doğrudan modelden oluşturuluyor.
        db.Database.EnsureCreated();
    }
    SeedData.Initialize(db);
}

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapAuthEndpoints();

app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
