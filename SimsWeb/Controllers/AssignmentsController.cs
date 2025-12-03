using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimsWeb.Services.Interfaces;
using SimsWeb.ViewModels;
using System.Security.Claims;

namespace SimsWeb.Controllers
{
    [Authorize]
    public class AssignmentsController : Controller
    {
        private readonly ITeacherAssignmentService _teacherService;
        private readonly IStudentAssignmentService _studentService;

        public AssignmentsController(
            ITeacherAssignmentService teacherService,
            IStudentAssignmentService studentService)
        {
            _teacherService = teacherService;
            _studentService = studentService;
        }

        // ========== INDEX ==========
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (User.IsInRole("Faculty"))
            {
                var list = await _teacherService.GetAssignmentsForTeacherAsync(userId);
                ViewData["Title"] = "My Assignments";
                return View("IndexFaculty", list);
            }

            if (User.IsInRole("Student"))
            {
                var list = await _studentService.GetAssignmentsForStudentAsync(userId);
                ViewData["Title"] = "My Assignments";
                return View("IndexStudent", list);
            }

            return Forbid();
        }

        // ========== CREATE (Faculty) ==========
        [Authorize(Roles = "Faculty")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vm = await _teacherService.BuildCreateViewModelAsync(userId);
            return View(vm);
        }

        [Authorize(Roles = "Faculty")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssignmentCreateViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!ModelState.IsValid)
            {
                var vm = await _teacherService.BuildCreateViewModelAsync(userId);
                model.TeacherClassSections = vm.TeacherClassSections;
                return View(model);
            }

            var assignmentId = await _teacherService.CreateAsync(model, userId);
            if (assignmentId == null)
            {
                ModelState.AddModelError("", "Could not create assignment.");
                var vm = await _teacherService.BuildCreateViewModelAsync(userId);
                model.TeacherClassSections = vm.TeacherClassSections;
                return View(model);
            }

            // ⬇⬇⬇ đổi từ Index sang Details
            return RedirectToAction("Details", new { id = assignmentId.Value });
        }


        // ========== DETAILS ==========
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (User.IsInRole("Faculty"))
            {
                var vm = await _teacherService.GetAssignmentDetailForTeacherAsync(id, userId);
                if (vm == null) return NotFound();
                ViewData["Title"] = "Assignment Details";
                return View("DetailsFaculty", vm);
            }

            if (User.IsInRole("Student"))
            {
                var vm = await _studentService.GetAssignmentDetailForStudentAsync(id, userId);
                if (vm == null) return NotFound();
                ViewData["Title"] = "Assignment Details";
                return View("DetailsStudent", vm);
            }

            return Forbid();
        }

        // ========== SUBMIT (Student) ==========
        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id, IFormFile file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ok = await _studentService.SubmitAsync(id, userId, file);

            TempData["AssignmentMessage"] = ok ? "Assignment submitted." : "Submit failed.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ========== GRADE (Faculty) ==========
        [Authorize(Roles = "Faculty")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grade(int submissionId, int score, string? comment, int assignmentId)
        {
            var ok = await _teacherService.GradeSubmissionAsync(submissionId, score, comment);

            TempData["AssignmentMessage"] = ok ? "Score updated." : "Grading failed.";
            return RedirectToAction(nameof(Details), new { id = assignmentId });
        }

        [Authorize(Roles = "Faculty")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vm = await _teacherService.BuildEditViewModelAsync(id, userId);
            if (vm == null) return NotFound();

            ViewData["Title"] = "Edit Assignment";
            return View(vm);
        }

        [Authorize(Roles = "Faculty")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AssignmentEditViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!ModelState.IsValid)
            {
                // cần load lại list lớp cho dropdown
                var createVm = await _teacherService.BuildCreateViewModelAsync(userId);
                model.TeacherClassSections = createVm.TeacherClassSections;
                ViewData["Title"] = "Edit Assignment";
                return View(model);
            }

            var ok = await _teacherService.UpdateAsync(model, userId);
            if (!ok) return NotFound();

            TempData["AssignmentMessage"] = "Assignment updated.";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        [Authorize(Roles = "Faculty")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ok = await _teacherService.SoftDeleteAsync(id, userId);

            TempData["AssignmentMessage"] = ok ? "Assignment deleted." : "Delete failed.";
            return RedirectToAction(nameof(Index));
        }


    }
}
