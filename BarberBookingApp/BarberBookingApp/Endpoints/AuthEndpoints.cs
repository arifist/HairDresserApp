using System.Security.Claims;
using BarberBookingApp.Data;
<<<<<<<< HEAD:BarberBookingApp/Endpoints/AuthEndpoints.cs
using BarberBookingApp.Helpers;
========
>>>>>>>> 4a268c4fe160d8d4f3270dd8111d60b2df812ab1:BarberBookingApp/BarberBookingApp/Endpoints/AuthEndpoints.cs
using BarberBookingApp.Models;
using BarberBookingApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BarberBookingApp.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/verify-otp", async (HttpContext http, AppDbContext db) =>
        {
            var form = http.Request.Form;
            var rawPhone = form["phoneNumber"].ToString();
            var phoneNumber = PhoneNumberHelper.Normalize(rawPhone);
            var fullName = form["fullName"].ToString().Trim();
            var returnUrl = form["returnUrl"].ToString();
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = "/randevularim";
            }

            if (!PhoneNumberHelper.IsValidTurkishMobile(rawPhone))
            {
                return Results.Redirect($"/giris?error=invalid-phone&returnUrl={Uri.EscapeDataString(returnUrl)}");
            }

            var customer = await db.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
            if (customer is null)
            {
                if (string.IsNullOrWhiteSpace(fullName))
                {
                    return Results.Redirect($"/giris?error=name-required&phone={Uri.EscapeDataString(phoneNumber)}&returnUrl={Uri.EscapeDataString(returnUrl)}");
                }

                customer = new Customer { PhoneNumber = phoneNumber, FullName = fullName };
                db.Customers.Add(customer);
                await db.SaveChangesAsync();
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, customer.Id.ToString()),
                new(ClaimTypes.Name, customer.FullName),
                new("PhoneNumber", customer.PhoneNumber),
                new(ClaimTypes.Role, "Customer")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30) });

            return Results.Redirect(returnUrl);
        });

        group.MapPost("/admin-login", async (HttpContext http, AppDbContext db) =>
        {
            var form = http.Request.Form;
            var username = form["username"].ToString().Trim();
            var password = form["password"].ToString();
            var returnUrl = form["returnUrl"].ToString();
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = "/admin/panel";
            }

            var admin = await db.Admins.FirstOrDefaultAsync(a => a.Username == username);
            if (admin is null)
            {
                return Results.Redirect("/admin/giris?error=1");
            }

            var hasher = new PasswordHasher<Admin>();
            var verifyResult = hasher.VerifyHashedPassword(admin, admin.PasswordHash, password);
            if (verifyResult == PasswordVerificationResult.Failed)
            {
                return Results.Redirect("/admin/giris?error=1");
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, admin.Id.ToString()),
                new(ClaimTypes.Name, admin.DisplayName),
                new(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(12) });

            return Results.Redirect(returnUrl);
        });

        group.MapPost("/logout", async (HttpContext http) =>
        {
            await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/");
        });
    }
}
