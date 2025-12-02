using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimsWeb.Services.Interfaces;
using SimsWeb.ViewModels;

namespace SimsWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FacultyController : Controller
    {
        private readonly IFacultyService _facultyService;

        public FacultyController(IFacultyService facultyService)
        {
            _facultyService = facultyService;
        }

        // =============== INDEX (active faculty) ===============
        public async Task<IActionResult> Index()
        {
            var model = await _facultyService.GetActiveAsync();
            return View(model);
        }

        // =============== CREATE (GET) ===============
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = await _facultyService.BuildCreateViewModelAsync();
            return View(vm);
        }

        // =============== CREATE (POST) ===============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FacultyEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _facultyService.BuildCreateViewModelAsync();
                model.FacultyUsers = vm.FacultyUsers;
                return View(model);
            }

            var (success, errors) = await _facultyService.CreateAsync(model);

            if (!success)
            {
                foreach (var e in errors)
                    ModelState.AddModelError("", e);

                var vm = await _facultyService.BuildCreateViewModelAsync();
                model.FacultyUsers = vm.FacultyUsers;

                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // =============== EDIT (GET) ===============
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vm = await _facultyService.BuildEditViewModelAsync(id);
            if (vm == null) return NotFound();

            return View(vm);
        }

        // =============== EDIT (POST) ===============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FacultyEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var vmReload = await _facultyService.BuildEditViewModelAsync(model.Id);
                if (vmReload != null)
                    model.FacultyUsers = vmReload.FacultyUsers;

                return View(model);
            }

            var (success, errors) = await _facultyService.UpdateAsync(model);

            if (!success)
            {
                foreach (var e in errors)
                    ModelState.AddModelError("", e);

                var vmReload = await _facultyService.BuildEditViewModelAsync(model.Id);
                if (vmReload != null)
                    model.FacultyUsers = vmReload.FacultyUsers;

                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // =============== SOFT DELETE ===============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _facultyService.SoftDeleteAsync(id);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(Index));
        }

        // =============== RECYCLE BIN LIST ===============
        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            var model = await _facultyService.GetDeletedAsync();
            return View(model);
        }

        // =============== RESTORE ===============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var ok = await _facultyService.RestoreAsync(id);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(Deleted));
        }

        // =============== HARD DELETE ===============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HardDelete(int id)
        {
            var ok = await _facultyService.HardDeleteAsync(id);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(Deleted));
        }
    }
}
