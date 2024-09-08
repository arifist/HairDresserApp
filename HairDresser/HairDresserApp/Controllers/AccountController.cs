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
                // Email adresine g�re kullan�c�y� buluyoruz
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

            // Kullan�c�n�n mevcut olup olmad���n� kontrol et
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                // Kullan�c� zaten varsa hata mesaj� g�ster
                ModelState.AddModelError("", "Bu e-posta zaten kay�tl�. L�tfen farkl� bir e-posta deneyin.");
                return View(model); // Kullan�c�y� VerifyPhoneNumber sayfas�na y�nlendirmeden formu tekrar g�ster
            }

            // Kullan�c� ad�n�n benzersizli�ini kontrol et
            var existingUserByUserName = await _userManager.FindByNameAsync(model.UserName);
            if (existingUserByUserName != null)
            {
                // Kullan�c� ad� zaten kay�tl�ysa hata mesaj� g�ster
                ModelState.AddModelError("", "Bu kullan�c� ad� zaten al�nm��. L�tfen farkl� bir kullan�c� ad� deneyin.");
                return View(model);
            }

            // �ifre kriterlerine uygun olup olmad���n� kontrol et
            var passwordValidationResult = ValidatePassword(model.Password);
            if (!passwordValidationResult.IsValid)
            {
                ModelState.AddModelError("Password", passwordValidationResult.ErrorMessage);
                return View(model); // �ifre uygun de�ilse formu tekrar g�ster
            }

            // Kullan�c�y� ge�ici olarak olu�tur (veritaban�na kaydetme)
            var user = new AppUser
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                FullName = model.FullName // Tam ad bilgisini burada ekliyoruz

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

        private (bool IsValid, string ErrorMessage) ValidatePassword(string password)
        {
            const int minLength = 6;

            if (password.Length < minLength)
            {
                return (false, $"�ifre en az {minLength} karakter uzunlu�unda olmal�d�r.");
            }

            if (!password.Any(char.IsUpper))
            {
                return (false, "�ifre en az bir b�y�k harf i�ermelidir.");
            }

            if (!password.Any(char.IsLower))
            {
                return (false, "�ifre en az bir k���k harf i�ermelidir.");
            }

            if (!password.Any(char.IsDigit))
            {
                return (false, "�ifre en az bir rakam i�ermelidir.");
            }

            return (true, string.Empty); // �ifre kriterlerine uygun
        }








        public IActionResult VerifyPhoneNumber()
        {
            // �lk a��ld���nda deneme hakk�n� 3 olarak ayarla
            TempData["AttemptsLeft"] = 3;
            TempData.Keep(); // TempData i�eri�ini sakla
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

                ModelState.AddModelError("", $"Yanl�� do�rulama kodu girdiniz. Deneme hakk�n�z {attemptsLeft}.");
            }

            return View();
        }







        public IActionResult AccessDenied([FromQuery(Name = "ReturnUrl")] string returUrl)
        {
            return View();
        }
    }

}