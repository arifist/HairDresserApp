using Entities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Contracts;

namespace HairDresserApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class WorkTimeController : Controller
    {
        private readonly IServiceManager _manager;

        public WorkTimeController(IServiceManager manager)
        {
            _manager = manager;
        }

        public IActionResult Index([FromRoute(Name = "id")] int id)
        {
            return View();
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Index([FromForm] WorkTimeDtoForUpdate workTimeDto)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _manager.WorkTimeService.UpdateOneWorkTime(workTimeDto);
        //        return RedirectToAction("Index");

        //    }
        //    return View();

        //}



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([FromForm] WorkTimeDtoForUpdate workTimeDto)
        {
            if (ModelState.IsValid)
            {
                _manager.WorkTimeService.UpdateOneWorkTime(workTimeDto);
                return Json(new { success = true, message = "Çalışma saatleri başarılı bir şekilde güncellendi!" });
            }
            return Json(new { success = false, message = "Güncelleme işlemi başarısız!" });
        }

    }
}
