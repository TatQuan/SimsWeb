using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimsWeb.Services.Interfaces;
using SimsWeb.ViewModels;
using System.Security.Claims;

namespace SimsWeb.Controllers
{

    public class EnrollmentsController : Controller
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly ICourseService _courseService;

        public EnrollmentsController(IEnrollmentService enrollmentService,ICourseService courseService)
        {
            _enrollmentService = enrollmentService;
            _courseService = courseService;
        }

        // =============== INDEX: Danh sách class để quản lý enroll ===============
        public async Task<IActionResult> Index(int? courseId)
        {
            var sections = await _enrollmentService.GetActiveClassSectionsAsync(courseId);
            var courses = await _courseService.GetAllAsync(); // ví dụ

            ViewBag.Courses = courses;
            ViewBag.SelectedCourseId = courseId;

            return View(sections);
        }


        // =============== MANAGE: Gán / bỏ gán student cho 1 class ===============
        [HttpGet]
        public async Task<IActionResult> Manage(int id)
        {
            var vm = await _enrollmentService.BuildManageViewModelAsync(id);
            if (vm == null) return NotFound();

            return View(vm);
        }


        // =============== ADD: thêm nhiều student vào class ===============
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudents(ClassEnrollmentViewModel model)
        {
            var (success, message) = await _enrollmentService
                .AddStudentsAsync(model.ClassSectionId, model.SelectedStudentIds ?? Array.Empty<int>());

            TempData["EnrollmentMessage"] = message;

            return RedirectToAction(nameof(Manage), new { id = model.ClassSectionId });
        }

        // =============== REMOVE: soft delete 1 student khỏi class ===============
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveStudent(int classSectionId, int studentId)
        {
            var (success, message) = await _enrollmentService.RemoveStudentAsync(classSectionId, studentId);

            TempData["EnrollmentMessage"] = message;

            return RedirectToAction(nameof(Manage), new { id = classSectionId });
        }
    }
}
