using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimsWeb.Data;
using SimsWeb.Services.Interfaces;
using SimsWeb.ViewModels;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimsWeb.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IScheduleService _scheduleService;

        public ProfileController(AppDbContext context, IScheduleService scheduleService)
        {
            _context = context;
            _scheduleService = scheduleService;
        }

        // Tự route theo role
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
                return await AdminProfile();

            if (User.IsInRole("Faculty"))
                return await FacultyProfile();


            if (User.IsInRole("Student"))
                return await StudentProfile();

            return Forbid();
        }

        // =============== STUDENT PROFILE ===============
        private async Task<IActionResult> StudentProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Enrollments.Where(e => !e.IsDeleted))
                    .ThenInclude(e => e.ClassSection)
                        .ThenInclude(cs => cs.Course)
                .Include(s => s.Enrollments.Where(e => !e.IsDeleted))
                    .ThenInclude(e => e.ClassSection)
                        .ThenInclude(cs => cs.Teacher)
                            .ThenInclude(f => f.User)
                .FirstOrDefaultAsync(s => s.UserId == userId && !s.IsDeleted);

            if (student == null) return NotFound("Student profile not found.");

            var currentClasses = student.Enrollments
                .Where(e => !e.IsDeleted && !e.ClassSection.IsDeleted)
                .Select(e => new StudentClassItemViewModel
                {
                    ClassSectionId = e.ClassSectionId,
                    ClassCode = e.ClassSection.Code,
                    ClassName = e.ClassSection.Name,
                    CourseCode = e.ClassSection.Course.Code,
                    CourseName = e.ClassSection.Course.Name,
                    Credits = e.ClassSection.Course.Credits ?? 0,
                    TeacherName = e.ClassSection.Teacher?.User?.FullName ?? "",
                    EnrolledAt = e.EnrolledAt
                })
                .OrderBy(c => c.ClassCode)
                .ToList();

            var totalCredits = currentClasses.Sum(c => c.Credits);

            var allSchedule = await _scheduleService.GetScheduleForStudentAsync(userId);
            var today = DateTime.Today.DayOfWeek;
            var todaySchedule = allSchedule
                .Where(s => s.DayOfWeek == today)
                .OrderBy(s => s.StartTime)
                .ToList();

            var vm = new StudentProfileViewModel
            {
                FullName = student.User?.FullName ?? "",
                Email = student.User?.Email ?? "",
                PhoneNumber = student.User?.PhoneNumber,
                AccountCreatedAt = student.User?.CreatedAt ?? DateTime.MinValue,

                StudentCode = student.StudentCode,
                StudentCreatedAt = student.CreatedAt,

                CurrentClassCount = currentClasses.Count,
                TotalCredits = totalCredits,
                CurrentClasses = currentClasses,
                TodaySchedule = todaySchedule
            };

            ViewData["Title"] = "My Profile";
            return View("Student", vm);
        }

        // =============== FACULTY PROFILE ===============
        private async Task<IActionResult> FacultyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1) Lấy faculty theo userId
            var faculty = await _context.Faculties   // <== DbSet tên "Faculties"
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.UserId == userId && !f.IsDeleted);

            if (faculty == null)
                return NotFound("Faculty profile not found.");

            // 2) Lấy danh sách ClassSection mà giảng viên này dạy
            var classSections = await _context.ClassSections
                .Where(cs => cs.TeacherId == faculty.Id && !cs.IsDeleted)
                .Include(cs => cs.Course)
                .Include(cs => cs.Enrollments)
                .ToListAsync();

            var currentClasses = classSections
                .Select(cs => new FacultyClassItemViewModel
                {
                    ClassSectionId = cs.Id,
                    ClassCode = cs.Code,
                    ClassName = cs.Name,
                    CourseCode = cs.Course.Code,
                    CourseName = cs.Course.Name,
                    Credits = cs.Course.Credits ?? 0,
                    StudentCount = cs.Enrollments.Count(e => !e.IsDeleted)
                })
                .OrderBy(c => c.ClassCode)
                .ToList();

            var totalCredits = currentClasses.Sum(c => c.Credits);
            var totalCourses = currentClasses.Select(c => c.CourseCode).Distinct().Count();

            // 3) Lấy lịch dạy hôm nay
            var allSchedule = await _scheduleService.GetScheduleForTeacherAsync(userId);
            var today = DateTime.Today.DayOfWeek;
            var todaySchedule = allSchedule
                .Where(s => s.DayOfWeek == today)
                .OrderBy(s => s.StartTime)
                .ToList();

            // 4) Build ViewModel
            var vm = new FacultyProfileViewModel
            {
                FullName = faculty.User?.FullName ?? "",
                Email = faculty.User?.Email ?? "",
                PhoneNumber = faculty.User?.PhoneNumber,
                AccountCreatedAt = faculty.User?.CreatedAt ?? DateTime.MinValue,

                FacultyCode = faculty.FacultyCode,
                Department = faculty.Department,
                Title = faculty.Title,
                FacultyCreatedAt = faculty.CreatedAt,

                CurrentClassCount = currentClasses.Count,
                TotalDistinctCourses = totalCourses,
                TotalCredits = totalCredits,
                CurrentClasses = currentClasses,
                TodaySchedule = todaySchedule
            };

            ViewData["Title"] = "My Profile";
            return View("Faculty", vm);
        }

        // =============== ADMIN PROFILE ===============
        private async Task<IActionResult> AdminProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

            if (user == null) return NotFound("Admin user not found.");

            var today = DateTime.Today.DayOfWeek;

            var vm = new AdminProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                AccountCreatedAt = user.CreatedAt,

                TotalStudents = await _context.Students.CountAsync(s => !s.IsDeleted),
                TotalFaculty = await _context.Faculties.CountAsync(f => !f.IsDeleted),
                TotalCourses = await _context.Courses.CountAsync(),
                TotalActiveClasses = await _context.ClassSections.CountAsync(cs => !cs.IsDeleted),
                TodayScheduleCount = await _context.ClassSchedules
                    .CountAsync(s => !s.IsDeleted && s.DayOfWeek == today)
            };

            ViewData["Title"] = "My Profile";
            return View("Admin", vm);
        }
    }
}
