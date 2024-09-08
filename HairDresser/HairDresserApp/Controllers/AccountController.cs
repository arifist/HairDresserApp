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

            // Kullan�c�y� ge�ici olarak olu�tur (veritaban�na kaydetme)
            var user = new IdentityUser
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
            };

            // SMS do�rulama kodu olu�tur ve g�nder
            var token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);
            try
            {
                await _smsSenderService.SendSMSAsync(user.PhoneNumber, $"Your verification code is: {token}");

                // Kullan�c� bilgilerini TempData'ya kaydet
                TempData["TempUser"] = JsonConvert.SerializeObject(model);
                TempData["Token"] = token; // Olu�turulan token'� sakla

                // Do�rulama sayfas�na y�nlendir
                return RedirectToAction("VerifyPhoneNumber");
            }
            catch (Exception ex)
            {
                // SMS g�nderimi ba�ar�s�z olursa hata mesaj� g�ster
                ModelState.AddModelError("", "SMS g�nderilemedi. L�tfen tekrar deneyin.");
                return View(model);
            }
        }


        public IActionResult VerifyPhoneNumber()
        {
            // �lk a��ld���nda deneme hakk�n� 3 olarak ayarla
            TempData["AttemptsLeft"] = 3;
            TempData.Keep(); // TempData i�eri�ini sakla
            return View();
        }




        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> VerifyPhoneNumber(string verificationCode)
        //{
        //    // Ge�ici kullan�c� verisini ve token'� al
        //    var tempUserData = TempData["TempUser"] as string;
        //    var originalToken = TempData["Token"] as string;

        //    // TempData'n�n s�resi dolmu�sa veya do�ru �ekilde al�namam��sa hata g�ster
        //    if (tempUserData == null || originalToken == null)
        //    {
        //        ModelState.AddModelError("", "Invalid verification process.");
        //        return View();
        //    }

        //    // Kullan�c� bilgilerini deserialize et
        //    var model = JsonConvert.DeserializeObject<RegisterDto>(tempUserData);
        //    var user = new IdentityUser
        //    {
        //        UserName = model.UserName,
        //        Email = model.Email,
        //        PhoneNumber = model.PhoneNumber,
        //    };

        //    // Girilen do�rulama kodunu olu�turulmu� token ile kar��la�t�r
        //    var isValid = string.Equals(originalToken, verificationCode, StringComparison.Ordinal);

        //    if (isValid)
        //    {
        //        // Kullan�c�y� veritaban�na kaydet
        //        var createResult = await _userManager.CreateAsync(user, model.Password);
        //        if (createResult.Succeeded)
        //        {
        //            // Kullan�c�y� "User" rol�ne ekle
        //            await _userManager.AddToRoleAsync(user, "User");

        //            // Telefon numaras�n� do�rula
        //            user.PhoneNumberConfirmed = true;
        //            await _userManager.UpdateAsync(user);

        //            // Ba�ar�l� kay�t sonras� giri� sayfas�na y�nlendir
        //            return RedirectToAction("Login");
        //        }
        //        else
        //        {
        //            // Kullan�c� olu�turulurken olu�an hatalar� g�ster
        //            foreach (var err in createResult.Errors)
        //            {
        //                ModelState.AddModelError("", err.Description);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        // Yanl�� do�rulama kodu girildi�inde hata mesaj� g�ster ve TempData'y� sakla
        //        ModelState.AddModelError("", "Yanl�� do�rulama kodu girdiniz. L�tfen tekrar deneyin.");

        //        // TempData i�eri�ini koru ve sakla
        //        TempData["TempUser"] = tempUserData;
        //        TempData["Token"] = originalToken;
        //        TempData.Keep();
        //    }

        //    // Kullan�c� kayd� ba�ar�s�z oldu�unda geri d�n��
        //    return View();
        //}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhoneNumber(string verificationCode)
        {
            // Ge�ici kullan�c� verisini ve token'� al
            var tempUserData = TempData["TempUser"] as string;
            var originalToken = TempData["Token"] as string;
            var attemptsLeft = TempData["AttemptsLeft"] as int? ?? 3; // Varsay�lan olarak 3 kullan

            // TempData'n�n s�resi dolmu�sa veya do�ru �ekilde al�namam��sa hata g�ster
            if (tempUserData == null || originalToken == null)
            {
                ModelState.AddModelError("", "Invalid verification process.");
                return View();
            }

            // Kullan�c� bilgilerini deserialize et
            var model = JsonConvert.DeserializeObject<RegisterDto>(tempUserData);
            var user = new IdentityUser
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
            };

            // Girilen do�rulama kodunu olu�turulmu� token ile kar��la�t�r
            var isValid = string.Equals(originalToken, verificationCode, StringComparison.Ordinal);

            if (isValid)
            {
                // Kullan�c�y� veritaban�na kaydet
                var createResult = await _userManager.CreateAsync(user, model.Password);
                if (createResult.Succeeded)
                {
                    // Kullan�c�y� "User" rol�ne ekle
                    await _userManager.AddToRoleAsync(user, "User");

                    // Telefon numaras�n� do�rula
                    user.PhoneNumberConfirmed = true;
                    await _userManager.UpdateAsync(user);

                    // Ba�ar�l� kay�t sonras� giri� sayfas�na y�nlendir
                    return RedirectToAction("Login");
                }
                else
                {
                    // Kullan�c� olu�turulurken olu�an hatalar� g�ster
                    foreach (var err in createResult.Errors)
                    {
                        ModelState.AddModelError("", err.Description);
                    }
                }
            }
            else
            {
                // Yanl�� do�rulama kodu girildi�inde hata mesaj� g�ster ve deneme hakk�n� g�ncelle
                attemptsLeft--;
                if (attemptsLeft <= 0)
                {
                    // Deneme hakk� kalmad�, kullan�c�y� kay�t sayfas�na y�nlendir
                    TempData["AttemptsLeft"] = attemptsLeft;
                    return RedirectToAction("Register");
                }

                // TempData i�eri�ini koru ve sakla
                TempData["TempUser"] = tempUserData;
                TempData["Token"] = originalToken;
                TempData["AttemptsLeft"] = attemptsLeft;
                TempData.Keep();

                // Kullan�c�ya deneme hakk� kalmad��� mesaj�n� g�ster
                ModelState.AddModelError("", $"Yanl�� do�rulama kodu girdiniz. Deneme hakk�n�z {attemptsLeft}.");
            }

            // Kullan�c� kayd� ba�ar�s�z oldu�unda geri d�n��
            return View();
        }







        public IActionResult AccessDenied([FromQuery(Name = "ReturnUrl")] string returUrl)
        {
            return View();
        }
    }
}