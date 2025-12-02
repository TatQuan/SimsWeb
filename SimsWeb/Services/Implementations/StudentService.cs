using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimsWeb.Data;
using SimsWeb.Models;
using SimsWeb.Services.Interfaces;
using SimsWeb.ViewModels;

namespace SimsWeb.Services.Implementations
{
    public class StudentService : IStudentService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public StudentService(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =============== LIST ACTIVE ===============
        public async Task<List<StudentListItemViewModel>> GetActiveAsync()
        {
            var students = await _context.Students
                .Where(s => !s.IsDeleted)
                .Include(s => s.User)
                .ToListAsync();

            return students.Select(s => new StudentListItemViewModel
            {
                Id = s.Id,
                StudentCode = s.StudentCode,
                FullName = s.User?.FullName ?? "",
                Email = s.User?.Email ?? "",
                CreatedAt = s.CreatedAt
            }).ToList();
        }

        // =============== LIST DELETED (RECYCLE BIN) ===============
        public async Task<List<StudentListItemViewModel>> GetDeletedAsync()
        {
            var students = await _context.Students
                .Where(s => s.IsDeleted)
                .Include(s => s.User)
                .ToListAsync();

            return students.Select(s => new StudentListItemViewModel
            {
                Id = s.Id,
                StudentCode = s.StudentCode,
                FullName = s.User?.FullName ?? "",
                Email = s.User?.Email ?? "",
                CreatedAt = s.CreatedAt
            }).ToList();
        }

        // ==== helper: nạp dropdown User + ClassSection cho ViewModel ====
        private async Task LoadDropdownsAsync(StudentEditViewModel model)
        {
            var studentUsers = await _userManager.GetUsersInRoleAsync("Student");

            model.StudentUsers = studentUsers
                .Where(u => !u.IsDeleted)
                .ToList();

            model.ClassSections = await _context.ClassSections
                .Where(c => !c.IsDeleted)
                .ToListAsync();
        }

        // =============== BUILD CREATE VM ===============
        public async Task<StudentEditViewModel> BuildCreateViewModelAsync()
        {
            var vm = new StudentEditViewModel();

            // users có role student
            var studentUsers = await _userManager.GetUsersInRoleAsync("Student");

            // loại user đã có student profile
            var existingStudentUserIds = await _context.Students
                .Select(s => s.UserId)
                .ToListAsync();

            vm.StudentUsers = studentUsers
                .Where(u => !u.IsDeleted && !existingStudentUserIds.Contains(u.Id))
                .ToList();

            vm.ClassSections = await _context.ClassSections
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            return vm;
        }

        // =============== CREATE ===============
        public async Task<(bool Success, IEnumerable<string> Errors)> CreateAsync(StudentEditViewModel model)
        {
            var errors = new List<string>();

            // user tồn tại?
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                errors.Add("Selected user not found.");
                return (false, errors);
            }

            // user đúng role Student?
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Student"))
            {
                errors.Add("Selected user is not in Student role.");
                return (false, errors);
            }

            // user đã có student profile chưa?
            var existed = await _context.Students.AnyAsync(s => s.UserId == model.UserId);
            if (existed)
            {
                errors.Add("This user already has a student profile.");
                return (false, errors);
            }

            var entity = new Student
            {
                UserId = model.UserId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Students.Add(entity);
            await _context.SaveChangesAsync();

            // StudentCode là NotMapped, generate từ Id, không cần set

            return (true, Enumerable.Empty<string>());
        }

        // =============== BUILD EDIT VM ===============
        public async Task<StudentEditViewModel?> BuildEditViewModelAsync(int id)
        {
            var entity = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (entity == null) return null;

            var vm = new StudentEditViewModel
            {
                Id = entity.Id,
                UserId = entity.UserId,
                DisplayStudentCode = entity.StudentCode
            };

            // nạp dropdown
            var studentUsers = await _userManager.GetUsersInRoleAsync("Student");

            vm.StudentUsers = studentUsers
                .Where(u => !u.IsDeleted)
                .ToList();

            vm.ClassSections = await _context.ClassSections
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            return vm;
        }

        // =============== UPDATE ===============
        public async Task<(bool Success, IEnumerable<string> Errors)> UpdateAsync(StudentEditViewModel model)
        {
            var errors = new List<string>();

            var entity = await _context.Students.FindAsync(model.Id);
            if (entity == null)
            {
                errors.Add("Student not found.");
                return (false, errors);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                errors.Add("Selected user not found.");
                return (false, errors);
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Student"))
            {
                errors.Add("Selected user is not in Student role.");
                return (false, errors);
            }

            // user này đã link tới student khác chưa?
            var duplicate = await _context.Students
                .AnyAsync(s => s.UserId == model.UserId && s.Id != model.Id);

            if (duplicate)
            {
                errors.Add("This user is already linked to another student.");
                return (false, errors);
            }

            entity.UserId = model.UserId;

            await _context.SaveChangesAsync();

            return (true, Enumerable.Empty<string>());
        }

        // =============== SOFT DELETE ===============
        public async Task<bool> SoftDeleteAsync(int id)
        {
            var entity = await _context.Students.FindAsync(id);
            if (entity == null) return false;

            entity.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        // =============== RESTORE ===============
        public async Task<bool> RestoreAsync(int id)
        {
            var entity = await _context.Students.FindAsync(id);
            if (entity == null) return false;

            entity.IsDeleted = false;
            await _context.SaveChangesAsync();
            return true;
        }

        // =============== HARD DELETE ===============
        public async Task<bool> HardDeleteAsync(int id)
        {
            var entity = await _context.Students.FindAsync(id);
            if (entity == null) return false;

            _context.Students.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
