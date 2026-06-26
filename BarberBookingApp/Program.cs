using System.Globalization;
using BarberBookingApp.Components;
using BarberBookingApp.Data;
using BarberBookingApp.Endpoints;
using BarberBookingApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Twilio;

var turkishCulture = new CultureInfo("tr-TR");
CultureInfo.DefaultThreadCurrentCulture = turkishCulture;
CultureInfo.DefaultThreadCurrentUICulture = turkishCulture;

var builder = WebApplication.CreateBuilder(args);

var containerPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(containerPort))
    builder.WebHost.UseUrls($"http://0.0.0.0:{containerPort}");

var twilioAccountSid = builder.Configuration["Twilio:AccountSid"];
var twilioAuthToken = builder.Configuration["Twilio:AuthToken"];
if (!string.IsNullOrWhiteSpace(twilioAccountSid) && !string.IsNullOrWhiteSpace(twilioAuthToken))
    TwilioClient.Init(twilioAccountSid, twilioAuthToken);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<JsonDataStore>();

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

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Singleton'ı startup sırasında oluştur (seed data'yı uygula)
app.Services.GetRequiredService<JsonDataStore>();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
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
