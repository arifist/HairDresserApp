using Microsoft.AspNetCore.Mvc;
using Services.Contracts;

namespace HairDresserApp.Controllers
{
    public class UserController: Controller
    {
        private readonly IServiceManager _manager;

        public UserController(IServiceManager manager)
        {
            _manager = manager;
        }

        public async Task<IActionResult> Index()
        {
            var userDto = await _manager.AuthService.GetUserDtoAsync(User);
            return View(userDto);
        }
    }
}
