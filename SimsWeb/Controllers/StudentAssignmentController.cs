using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimsWeb.Data;
using SimsWeb.Services.Interfaces;
using SimsWeb.ViewModels.Assignments;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimsWeb.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentAssignmentController : Controller
    {
        private readonly IStudentAssignmentService _assignmentService;
        private readonly AppDbContext _db;

        public StudentAssignmentController(
            IStudentAssignmentService assignmentService,
            AppDbContext db)
        {
            _assignmentService = assignmentService;
            _db = db;
        }

        // Danh sách các lớp mà student đang học
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var student = await _db.Students
                .FirstOrDefaultAsync(s => s.UserId == userId && !s.IsDeleted);
            if (student == null) return Forbid();

            var model = await _assignmentService.GetStudentClassesAsync(student.Id);
            ViewData["Title"] = "My Classes";
            return View(model);
        }

        // Danh sách assignment của 1 lớp (dù chưa có assignment vẫn vào bình thường)
        public async Task<IActionResult> ClassAssignments(int classSectionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var student = await _db.Students
                .FirstOrDefaultAsync(s => s.UserId == userId && !s.IsDeleted);
            if (student == null) return Forbid();

            // kiểm tra student có thuộc lớp này không
            var enrolled = await _db.Enrollments
                .AnyAsync(e => e.StudentId == student.Id &&
                               e.ClassSectionId == classSectionId &&
                               !e.IsDeleted);

            if (!enrolled) return Forbid();

            var cls = await _db.ClassSections
                .Include(cs => cs.Course)
                .Include(cs => cs.Assignments)
                .FirstOrDefaultAsync(cs => cs.Id == classSectionId && !cs.IsDeleted);

            if (cls == null) return NotFound();

            var vm = new StudentClassAssignmentsVM
            {
                ClassSectionId = cls.Id,
                ClassName = cls.Name ?? cls.Code,
                CourseName = cls.Course.Name,
                Assignments = cls.Assignments
                    .Where(a => !a.IsDeleted)
                    .OrderByDescending(a => a.DueAt)
                    .Select(a => new AssignmentListItemVM
                    {
                        Id = a.Id,
                        Title = a.Title,
                        DueAt = a.DueAt,
                        IsDeleted = a.IsDeleted
                    })
                    .ToList()
            };

            ViewData["Title"] = "Class Assignments";
            return View(vm);
        }

        // Chi tiết 1 assignment + Documents + Brief + MySubmission
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var student = await _db.Students
                .FirstOrDefaultAsync(s => s.UserId == userId && !s.IsDeleted);
            if (student == null) return Forbid();

            var vm = await _assignmentService.GetAssignmentDetailAsync(id, student.Id);
            ViewData["Title"] = "Assignment Details";
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Submit(SubmitAssignmentVM model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var student = await _db.Students
                .FirstOrDefaultAsync(s => s.UserId == userId && !s.IsDeleted);
            if (student == null) return Forbid();

            if (!ModelState.IsValid)
            {
                // nếu form lỗi, quay lại Details
                var vmError = await _assignmentService.GetAssignmentDetailAsync(model.AssignmentId, student.Id);
                ViewData["Title"] = "Assignment Details";
                return View("Details", vmError);
            }

            await _assignmentService.SubmitAsync(model, student.Id);

            return RedirectToAction(nameof(Details), new { id = model.AssignmentId });
        }
    }
}
