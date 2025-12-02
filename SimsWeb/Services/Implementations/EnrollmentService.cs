using Microsoft.EntityFrameworkCore;
using SimsWeb.Data;
using SimsWeb.Models;
using SimsWeb.Services.Interfaces;
using SimsWeb.ViewModels;

namespace SimsWeb.Services.Implementations
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly AppDbContext _context;

        public EnrollmentService(AppDbContext context)
        {
            _context = context;
        }

        // =============== Lấy danh sách class còn active cho trang Index ===============
        //public async Task<List<ClassSection>> GetActiveClassSectionsAsync()
        //{
        //    var sections = await _context.ClassSections
        //        .Where(c => !c.IsDeleted)
        //        .Include(c => c.Course)
        //        .Include(c => c.Teacher)
        //            .ThenInclude(f => f.User)
        //        .OrderBy(c => c.Code)
        //        .ToListAsync();

        //    return sections;
        //}

        public async Task<List<ClassSection>> GetActiveClassSectionsAsync(int? courseId = null)
        {
            var query = _context.ClassSections
                .Where(c => !c.IsDeleted);

            if (courseId.HasValue)
            {
                query = query.Where(c => c.CourseId == courseId.Value);
            }

            var sections = await query
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                    .ThenInclude(f => f.User)
                .OrderBy(c => c.Code)
                .ToListAsync();

            return sections;
        }


        // =============== Build ViewModel cho trang Manage ===============
        public async Task<ClassEnrollmentViewModel?> BuildManageViewModelAsync(int classSectionId)
        {
            var section = await _context.ClassSections
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                    .ThenInclude(f => f.User)
                .FirstOrDefaultAsync(c => c.Id == classSectionId && !c.IsDeleted);

            if (section == null) return null;

            // Enrollments active của class này
            var enrollments = await _context.Enrollments
                .Where(e => e.ClassSectionId == classSectionId && !e.IsDeleted)
                .Include(e => e.Student).ThenInclude(s => s.User)
                .ToListAsync();

            var enrolledStudentIds = enrollments.Select(e => e.StudentId).ToList();

            // Students khả dụng: chưa xóa & chưa được enroll vào class này
            var availableStudents = await _context.Students
                .Where(s => !s.IsDeleted && !enrolledStudentIds.Contains(s.Id))
                .Include(s => s.User)
                .ToListAsync();

            var vm = new ClassEnrollmentViewModel
            {
                ClassSectionId = section.Id,
                ClassCode = section.Code,
                ClassName = section.Name,
                CourseName = section.Course?.Name ?? "",
                TeacherName = section.Teacher?.User?.FullName ?? "",

                EnrolledStudents = enrollments.Select(e => new ClassEnrollmentStudentItemViewModel
                {
                    StudentId = e.StudentId,
                    StudentCode = e.Student.StudentCode,
                    FullName = e.Student.User?.FullName ?? "",
                    Email = e.Student.User?.Email ?? ""
                }).ToList(),

                AvailableStudents = availableStudents.Select(s => new ClassEnrollmentStudentItemViewModel
                {
                    StudentId = s.Id,
                    StudentCode = s.StudentCode,
                    FullName = s.User?.FullName ?? "",
                    Email = s.User?.Email ?? ""
                }).ToList()
            };

            return vm;
        }

        // =============== Thêm nhiều student vào class ===============
        public async Task<(bool Success, string Message)> AddStudentsAsync(int classSectionId, int[] selectedStudentIds)
        {
            if (selectedStudentIds == null || selectedStudentIds.Length == 0)
            {
                return (false, "No students selected.");
            }

            var ids = selectedStudentIds.Distinct().ToList();

            // lấy các enrollment hiện có (kể cả đã IsDeleted)
            var existingEnrollments = await _context.Enrollments
                .Where(e => e.ClassSectionId == classSectionId && ids.Contains(e.StudentId))
                .ToListAsync();

            var now = DateTime.UtcNow;
            var createdCount = 0;
            var restoredCount = 0;

            foreach (var sid in ids)
            {
                var exist = existingEnrollments.FirstOrDefault(e => e.StudentId == sid);

                if (exist == null)
                {
                    var newEnroll = new Enrollment
                    {
                        ClassSectionId = classSectionId,
                        StudentId = sid,
                        EnrolledAt = now,
                        IsDeleted = false
                    };
                    _context.Enrollments.Add(newEnroll);
                    createdCount++;
                }
                else if (exist.IsDeleted)
                {
                    exist.IsDeleted = false;
                    exist.EnrolledAt = now;
                    restoredCount++;
                }
                // nếu exist != null && !IsDeleted => đã enroll active, bỏ qua
            }

            await _context.SaveChangesAsync();

            var msg = $"Added {createdCount} new, restored {restoredCount} enrollment(s).";
            return (true, msg);
        }

        // =============== Bỏ gán 1 student khỏi class (soft delete) ===============
        public async Task<(bool Success, string Message)> RemoveStudentAsync(int classSectionId, int studentId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.ClassSectionId == classSectionId
                                          && e.StudentId == studentId
                                          && !e.IsDeleted);

            if (enrollment == null)
            {
                return (false, "Enrollment not found or already removed.");
            }

            enrollment.IsDeleted = true;
            await _context.SaveChangesAsync();

            return (true, "Student removed from class.");
        }
    }
}
