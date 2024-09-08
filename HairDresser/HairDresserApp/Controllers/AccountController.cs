using Entities.Dtos;
using Entities.Models;
using HairDresserApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.Contracts;

namespace HairDresserApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ISMSSenderService _smsSenderService;

        public AccountController(UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ISMSSenderService smsSenderService) // SMS servisini ekleyin

        {
            _userManager = userManager;
            _signInManager = signInManager;
            _smsSenderService = smsSenderService;

        }

        public IActionResult Login([FromQuery(Name = "ReturnUrl")] string? ReturnUrl = "/")
        {
            return View(new LoginModel()
            {
                ReturnUrl = ReturnUrl ?? "/"
            });
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Login([FromForm] LoginModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        AppUser user = await _userManager.FindByNameAsync(model.Name);
        //        if (user is not null)
        //        {
        //            await _signInManager.SignOutAsync();
        //            if ((await _signInManager.PasswordSignInAsync(user, model.Password, false, false)).Succeeded)
        //            {
        //                return Redirect(model?.ReturnUrl ?? "/");
        //            }
        //        }
        //        ModelState.AddModelError("Error", "Invalid username or password.");
        //    }
        //    return View();
        //}


        // AccountController.cs
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] LoginModel model)
        {
            if (ModelState.IsValid)
            {
                // Email adresine göre kullanýcýyý buluyoruz
                AppUser user = await _userManager.FindByEmailAsync(model.Email);
                if (user is not null)
                {
                    await _signInManager.SignOutAsync();
                    if ((await _signInManager.PasswordSignInAsync(user, model.Password, false, false)).Succeeded)
                    {
                        return Redirect(model?.ReturnUrl ?? "/");
                    }
                }
                ModelState.AddModelError("Error", "Invalid email or password.");
            }
            return View(model);
        }





        public async Task<IActionResult> Logout([FromQuery(Name = "ReturnUrl")] string ReturnUrl = "/")
        {
            await _signInManager.SignOutAsync();
            return Redirect(ReturnUrl);
        }

        public IActionResult Register()
        {

            return View();
        }






        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromForm] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Kullanýcýnýn mevcut olup olmadýðýný kontrol et
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                // Kullanýcý zaten varsa hata mesajý göster
                ModelState.AddModelError("", "Bu e-posta zaten kayýtlý. Lütfen farklý bir e-posta deneyin.");
                return View(model); // Kullanýcýyý VerifyPhoneNumber sayfasýna yönlendirmeden formu tekrar göster
            }

            // Kullanýcý adýnýn benzersizliðini kontrol et
            var existingUserByUserName = await _userManager.FindByNameAsync(model.UserName);
            if (existingUserByUserName != null)
            {
                // Kullanýcý adý zaten kayýtlýysa hata mesajý göster
                ModelState.AddModelError("", "Bu kullanýcý adý zaten alýnmýþ. Lütfen farklý bir kullanýcý adý deneyin.");
                return View(model);
            }

            // Þifre kriterlerine uygun olup olmadýðýný kontrol et
            var passwordValidationResult = ValidatePassword(model.Password);
            if (!passwordValidationResult.IsValid)
            {
                ModelState.AddModelError("Password", passwordValidationResult.ErrorMessage);
                return View(model); // Þifre uygun deðilse formu tekrar göster
            }

            // Kullanýcýyý geçici olarak oluþtur (veritabanýna kaydetme)
            var user = new AppUser
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                FullName = model.FullName // Tam ad bilgisini burada ekliyoruz

            };

            // SMS doðrulama kodu oluþtur ve gönder
            var token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);
            try
            {
                await _smsSenderService.SendSMSAsync(user.PhoneNumber, $"Your verification code is: {token}");

                // Kullanýcý bilgilerini TempData'ya kaydet
                TempData["TempUser"] = JsonConvert.SerializeObject(model);
                TempData["Token"] = token; // Oluþturulan token'ý sakla

                // Doðrulama sayfasýna yönlendir
                return RedirectToAction("VerifyPhoneNumber");
            }
            catch (Exception ex)
            {
                // SMS gönderimi baþarýsýz olursa hata mesajý göster
                ModelState.AddModelError("", "SMS gönderilemedi. Lütfen tekrar deneyin.");
                return View(model);
            }
        }

        private (bool IsValid, string ErrorMessage) ValidatePassword(string password)
        {
            const int minLength = 6;

            if (password.Length < minLength)
            {
                return (false, $"Þifre en az {minLength} karakter uzunluðunda olmalýdýr.");
            }

            if (!password.Any(char.IsUpper))
            {
                return (false, "Þifre en az bir büyük harf içermelidir.");
            }

            if (!password.Any(char.IsLower))
            {
                return (false, "Þifre en az bir küçük harf içermelidir.");
            }

            if (!password.Any(char.IsDigit))
            {
                return (false, "Þifre en az bir rakam içermelidir.");
            }

            return (true, string.Empty); // Þifre kriterlerine uygun
        }








        public IActionResult VerifyPhoneNumber()
        {
            // Ýlk açýldýðýnda deneme hakkýný 3 olarak ayarla
            TempData["AttemptsLeft"] = 3;
            TempData.Keep(); // TempData içeriðini sakla
            return View();
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhoneNumber(string verificationCode)
        {
            var tempUserData = TempData["TempUser"] as string;
            var originalToken = TempData["Token"] as string;
            var attemptsLeft = TempData["AttemptsLeft"] as int? ?? 3;

            if (tempUserData == null || originalToken == null)
            {
                ModelState.AddModelError("", "Invalid verification process.");
                return View();
            }

            var model = JsonConvert.DeserializeObject<RegisterDto>(tempUserData);
            var user = new AppUser
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                FullName=model.FullName

            };

            var isValid = string.Equals(originalToken, verificationCode, StringComparison.Ordinal);

            if (isValid)
            {
                var createResult = await _userManager.CreateAsync(user, model.Password);
                if (createResult.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");
                    user.PhoneNumberConfirmed = true;
                    await _userManager.UpdateAsync(user);

                    return RedirectToAction("Login");
                }
                else
                {
                    foreach (var err in createResult.Errors)
                    {
                        ModelState.AddModelError("", err.Description);
                    }
                }
            }
            else
            {
                attemptsLeft--;
                if (attemptsLeft <= 0)
                {
                    TempData["AttemptsLeft"] = attemptsLeft;
                    return RedirectToAction("Register");
                }

                TempData["TempUser"] = tempUserData;
                TempData["Token"] = originalToken;
                TempData["AttemptsLeft"] = attemptsLeft;
                TempData.Keep();

                ModelState.AddModelError("", $"Yanlýþ doðrulama kodu girdiniz. Deneme hakkýnýz {attemptsLeft}.");
            }

            return View();
        }







        public IActionResult AccessDenied([FromQuery(Name = "ReturnUrl")] string returUrl)
        {
            return View();
        }
    }

}