using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimsWeb.Services.Interfaces;
using System.Security.Claims;

namespace SimsWeb.Controllers
{
    [Authorize]
    public class SchedulesController : Controller
    {
        private readonly IScheduleService _scheduleService;

        public SchedulesController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (User.IsInRole("Faculty"))
            {
                var model = await _scheduleService.GetScheduleForTeacherAsync(userId);
                ViewData["Title"] = "My Teaching Schedule";
                return View(model);
            }

            if (User.IsInRole("Student"))
            {
                var model = await _scheduleService.GetScheduleForStudentAsync(userId);
                ViewData["Title"] = "My Class Schedule";
                return View(model);
            }

            // Không phải Student/Faculty thì cho về Home hoặc Admin, tùy cậu
            if (User.IsInRole("Admin"))
            {
                // Ví dụ redirect về trang quản lý lịch admin
                return RedirectToAction("Index", "ClassSchedules");
            }

            return Forbid();
        }
    }
}
