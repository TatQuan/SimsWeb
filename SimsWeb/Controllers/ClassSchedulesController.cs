using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimsWeb.Helpers;
using SimsWeb.Models;
using SimsWeb.Services.Interfaces;
using SimsWeb.ViewModels;

namespace SimsWeb.Controllers
{
    [Authorize(Roles = "Admin,Faculty")]
    public class ClassSchedulesController : Controller
    {
        private readonly IScheduleService _scheduleService;

        public ClassSchedulesController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        // =============== DANH SÁCH LỊCH HOẠT ĐỘNG ===============
        public async Task<IActionResult> Index()
        {
            // Admin xem tất cả schedule (chưa xóa)
            var schedules = await _scheduleService.GetActiveSchedulesAsync();
            return View(schedules); // model: List<ClassSchedule>
        }

        // =============== RECYCLE BIN ===============
        public async Task<IActionResult> Deleted()
        {
            var schedules = await _scheduleService.GetDeletedSchedulesAsync();
            return View(schedules); // model: List<ClassSchedule>
        }

        // =============== CREATE (GET) ===============
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = await _scheduleService.BuildCreateViewModelAsync();
            return View(vm);
        }

        // =============== CREATE (POST) ===============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassScheduleViewModel model)
        {
            // Map Period -> Start/End theo config
            var (start, end) = ScheduleTimeHelper.GetPeriodTime(model.Period);
            model.StartTime = start;
            model.EndTime = end;

            if (!ModelState.IsValid)
            {
                var vm = await _scheduleService.BuildCreateViewModelAsync();
                model.ClassSections = vm.ClassSections;
                return View(model);
            }

            await _scheduleService.CreateAsync(model);
            return RedirectToAction(nameof(Index));
        }

        // =============== EDIT (GET) ===============
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vm = await _scheduleService.BuildEditViewModelAsync(id);
            if (vm == null) return NotFound();

            return View(vm);
        }

        // =============== EDIT (POST) ===============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClassScheduleViewModel model)
        {
            var (start, end) = ScheduleTimeHelper.GetPeriodTime(model.Period);
            model.StartTime = start;
            model.EndTime = end;

            if (!ModelState.IsValid)
            {
                var vm = await _scheduleService.BuildCreateViewModelAsync();
                model.ClassSections = vm.ClassSections;
                return View(model);
            }

            var ok = await _scheduleService.UpdateAsync(model);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(Index));
        }


        // =============== SOFT DELETE ===============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _scheduleService.SoftDeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // =============== RESTORE ===============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            await _scheduleService.RestoreAsync(id);
            return RedirectToAction(nameof(Deleted));
        }

        // =============== HARD DELETE ===============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HardDelete(int id)
        {
            await _scheduleService.HardDeleteAsync(id);
            return RedirectToAction(nameof(Deleted));
        }
    }
}
