using Entities.Dtos;
using HairDresserApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.Contracts;

namespace HairDresserApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ISMSSenderService _smsSenderService;

        public AccountController(UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] LoginModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityUser user = await _userManager.FindByNameAsync(model.Name);
                if (user is not null)
                {
                    await _signInManager.SignOutAsync();
                    if ((await _signInManager.PasswordSignInAsync(user, model.Password, false, false)).Succeeded)
                    {
                        return Redirect(model?.ReturnUrl ?? "/");
                    }
                }
                ModelState.AddModelError("Error", "Invalid username or password.");
            }
            return View();
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

            // Kullanýcýyý geçici olarak oluþtur (veritabanýna kaydetme)
            var user = new IdentityUser
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
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


        public IActionResult VerifyPhoneNumber()
        {
            // Ýlk açýldýðýnda deneme hakkýný 3 olarak ayarla
            TempData["AttemptsLeft"] = 3;
            TempData.Keep(); // TempData içeriðini sakla
            return View();
        }




        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> VerifyPhoneNumber(string verificationCode)
        //{
        //    // Geçici kullanýcý verisini ve token'ý al
        //    var tempUserData = TempData["TempUser"] as string;
        //    var originalToken = TempData["Token"] as string;

        //    // TempData'nýn süresi dolmuþsa veya doðru þekilde alýnamamýþsa hata göster
        //    if (tempUserData == null || originalToken == null)
        //    {
        //        ModelState.AddModelError("", "Invalid verification process.");
        //        return View();
        //    }

        //    // Kullanýcý bilgilerini deserialize et
        //    var model = JsonConvert.DeserializeObject<RegisterDto>(tempUserData);
        //    var user = new IdentityUser
        //    {
        //        UserName = model.UserName,
        //        Email = model.Email,
        //        PhoneNumber = model.PhoneNumber,
        //    };

        //    // Girilen doðrulama kodunu oluþturulmuþ token ile karþýlaþtýr
        //    var isValid = string.Equals(originalToken, verificationCode, StringComparison.Ordinal);

        //    if (isValid)
        //    {
        //        // Kullanýcýyý veritabanýna kaydet
        //        var createResult = await _userManager.CreateAsync(user, model.Password);
        //        if (createResult.Succeeded)
        //        {
        //            // Kullanýcýyý "User" rolüne ekle
        //            await _userManager.AddToRoleAsync(user, "User");

        //            // Telefon numarasýný doðrula
        //            user.PhoneNumberConfirmed = true;
        //            await _userManager.UpdateAsync(user);

        //            // Baþarýlý kayýt sonrasý giriþ sayfasýna yönlendir
        //            return RedirectToAction("Login");
        //        }
        //        else
        //        {
        //            // Kullanýcý oluþturulurken oluþan hatalarý göster
        //            foreach (var err in createResult.Errors)
        //            {
        //                ModelState.AddModelError("", err.Description);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        // Yanlýþ doðrulama kodu girildiðinde hata mesajý göster ve TempData'yý sakla
        //        ModelState.AddModelError("", "Yanlýþ doðrulama kodu girdiniz. Lütfen tekrar deneyin.");

        //        // TempData içeriðini koru ve sakla
        //        TempData["TempUser"] = tempUserData;
        //        TempData["Token"] = originalToken;
        //        TempData.Keep();
        //    }

        //    // Kullanýcý kaydý baþarýsýz olduðunda geri dönüþ
        //    return View();
        //}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhoneNumber(string verificationCode)
        {
            // Geçici kullanýcý verisini ve token'ý al
            var tempUserData = TempData["TempUser"] as string;
            var originalToken = TempData["Token"] as string;
            var attemptsLeft = TempData["AttemptsLeft"] as int? ?? 3; // Varsayýlan olarak 3 kullan

            // TempData'nýn süresi dolmuþsa veya doðru þekilde alýnamamýþsa hata göster
            if (tempUserData == null || originalToken == null)
            {
                ModelState.AddModelError("", "Invalid verification process.");
                return View();
            }

            // Kullanýcý bilgilerini deserialize et
            var model = JsonConvert.DeserializeObject<RegisterDto>(tempUserData);
            var user = new IdentityUser
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
            };

            // Girilen doðrulama kodunu oluþturulmuþ token ile karþýlaþtýr
            var isValid = string.Equals(originalToken, verificationCode, StringComparison.Ordinal);

            if (isValid)
            {
                // Kullanýcýyý veritabanýna kaydet
                var createResult = await _userManager.CreateAsync(user, model.Password);
                if (createResult.Succeeded)
                {
                    // Kullanýcýyý "User" rolüne ekle
                    await _userManager.AddToRoleAsync(user, "User");

                    // Telefon numarasýný doðrula
                    user.PhoneNumberConfirmed = true;
                    await _userManager.UpdateAsync(user);

                    // Baþarýlý kayýt sonrasý giriþ sayfasýna yönlendir
                    return RedirectToAction("Login");
                }
                else
                {
                    // Kullanýcý oluþturulurken oluþan hatalarý göster
                    foreach (var err in createResult.Errors)
                    {
                        ModelState.AddModelError("", err.Description);
                    }
                }
            }
            else
            {
                // Yanlýþ doðrulama kodu girildiðinde hata mesajý göster ve deneme hakkýný güncelle
                attemptsLeft--;
                if (attemptsLeft <= 0)
                {
                    // Deneme hakký kalmadý, kullanýcýyý kayýt sayfasýna yönlendir
                    TempData["AttemptsLeft"] = attemptsLeft;
                    return RedirectToAction("Register");
                }

                // TempData içeriðini koru ve sakla
                TempData["TempUser"] = tempUserData;
                TempData["Token"] = originalToken;
                TempData["AttemptsLeft"] = attemptsLeft;
                TempData.Keep();

                // Kullanýcýya deneme hakký kalmadýðý mesajýný göster
                ModelState.AddModelError("", $"Yanlýþ doðrulama kodu girdiniz. Deneme hakkýnýz {attemptsLeft}.");
            }

            // Kullanýcý kaydý baþarýsýz olduðunda geri dönüþ
            return View();
        }







        public IActionResult AccessDenied([FromQuery(Name = "ReturnUrl")] string returUrl)
        {
            return View();
        }
    }
}