using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimsWeb.Services.Interfaces;
using SimsWeb.ViewModels;

namespace SimsWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StudentsController : Controller
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        // =============== INDEX (active students) ===============
        public async Task<IActionResult> Index()
        {
            var model = await _studentService.GetActiveAsync();
            return View(model);
        }

        // =============== CREATE (GET) ===============
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = await _studentService.BuildCreateViewModelAsync();
            return View(vm);
        }

        // =============== CREATE (POST) ===============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var vmReload = await _studentService.BuildCreateViewModelAsync();
                model.StudentUsers = vmReload.StudentUsers;
                model.ClassSections = vmReload.ClassSections;
                return View(model);
            }

            var (success, errors) = await _studentService.CreateAsync(model);

            if (!success)
            {
                foreach (var e in errors)
                    ModelState.AddModelError("", e);

                var vmReload = await _studentService.BuildCreateViewModelAsync();
                model.StudentUsers = vmReload.StudentUsers;
                model.ClassSections = vmReload.ClassSections;
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // =============== EDIT (GET) ===============
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vm = await _studentService.BuildEditViewModelAsync(id);
            if (vm == null) return NotFound();

            return View(vm);
        }

        // =============== EDIT (POST) ===============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StudentEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var vmReload = await _studentService.BuildEditViewModelAsync(model.Id);
                if (vmReload != null)
                {
                    model.StudentUsers = vmReload.StudentUsers;
                    model.ClassSections = vmReload.ClassSections;
                }
                return View(model);
            }

            var (success, errors) = await _studentService.UpdateAsync(model);

            if (!success)
            {
                foreach (var e in errors)
                    ModelState.AddModelError("", e);

                var vmReload = await _studentService.BuildEditViewModelAsync(model.Id);
                if (vmReload != null)
                {
                    model.StudentUsers = vmReload.StudentUsers;
                    model.ClassSections = vmReload.ClassSections;
                }

                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // =============== SOFT DELETE ===============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _studentService.SoftDeleteAsync(id);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(Index));
        }

        // =============== RECYCLE BIN LIST ===============
        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            var model = await _studentService.GetDeletedAsync();
            return View(model);
        }

        // =============== RESTORE ===============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var ok = await _studentService.RestoreAsync(id);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(Deleted));
        }

        // =============== HARD DELETE ===============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HardDelete(int id)
        {
            var ok = await _studentService.HardDeleteAsync(id);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(Deleted));
        }
    }
}
