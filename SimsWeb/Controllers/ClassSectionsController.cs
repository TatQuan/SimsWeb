using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SimsWeb.Services.Interfaces;
using SimsWeb.ViewModels;

namespace SimsWeb.Controllers
{

    public class ClassSectionsController : Controller
    {
        private readonly IClassSectionService _classSectionService;

        public ClassSectionsController(IClassSectionService classSectionService)
        {
            _classSectionService = classSectionService;
        }

        // =============== INDEX: danh sách lớp chưa xóa ===============
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                var model = await _classSectionService.GetActiveSectionsAsync();
                return View(model);
            }

            if (User.IsInRole("Faculty"))
            {
                //TODO: Get list of ClassSection that logger is assigned
            }
            else if (User.IsInRole("Student"))

            {
                //TODO: Get list of ClassSection that logger is assigned
            }

            return View();
        }

        // =============== CREATE (GET) ===============
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = await _classSectionService.BuildCreateViewModelAsync();
            return View(vm);
        }

        // =============== CREATE (POST) ===============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassSectionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // load lại dropdown
                var vm = await _classSectionService.BuildCreateViewModelAsync();
                model.Courses = vm.Courses;
                model.Teachers = vm.Teachers;
                return View(model);
            }

            var (success, errors) = await _classSectionService.CreateAsync(model);

            if (!success)
            {
                foreach (var e in errors)
                    ModelState.AddModelError("", e);

                var vm = await _classSectionService.BuildCreateViewModelAsync();
                model.Courses = vm.Courses;
                model.Teachers = vm.Teachers;

                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // =============== EDIT (GET) ===============
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vm = await _classSectionService.BuildEditViewModelAsync(id);
            if (vm == null) return NotFound();

            return View(vm);
        }

        // =============== EDIT (POST) ===============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClassSectionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _classSectionService.BuildEditViewModelAsync(model.Id);
                if (vm != null)
                {
                    model.Courses = vm.Courses;
                    model.Teachers = vm.Teachers;
                }
                return View(model);
            }

            var (success, errors) = await _classSectionService.UpdateAsync(model);

            if (!success)
            {
                foreach (var e in errors)
                    ModelState.AddModelError("", e);

                var vm = await _classSectionService.BuildEditViewModelAsync(model.Id);
                if (vm != null)
                {
                    model.Courses = vm.Courses;
                    model.Teachers = vm.Teachers;
                }

                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // =============== SOFT DELETE ===============
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _classSectionService.SoftDeleteAsync(id);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(Index));
        }

        // =============== RECYCLE BIN: danh sách đã xóa mềm ===============
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            var model = await _classSectionService.GetDeletedSectionsAsync();
            return View(model);
        }

        // =============== RESTORE ===============
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var ok = await _classSectionService.RestoreAsync(id);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(Deleted));
        }

        // =============== HARD DELETE ===============
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HardDelete(int id)
        {
            var ok = await _classSectionService.HardDeleteAsync(id);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(Deleted));
        }
    }
}
