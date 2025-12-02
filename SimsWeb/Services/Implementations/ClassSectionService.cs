using Microsoft.EntityFrameworkCore;
using SimsWeb.Data;
using SimsWeb.Models;
using SimsWeb.Services.Interfaces;
using SimsWeb.ViewModels;

namespace SimsWeb.Services.Implementations
{
    public class ClassSectionService : IClassSectionService
    {
        private readonly AppDbContext _context;

        public ClassSectionService(AppDbContext context)
        {
            _context = context;
        }

        // ========= LIST ACTIVE =========
        public async Task<List<ClassSectionListItemViewModel>> GetActiveSectionsAsync()
        {
            var sections = await _context.ClassSections
                .Where(c => !c.IsDeleted)
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                    .ThenInclude(f => f.User)
                .ToListAsync();

            return sections.Select(c => new ClassSectionListItemViewModel
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                CourseName = c.Course?.Name ?? "",
                TeacherName = c.Teacher != null ? c.Teacher.User.FullName : "",
                CreatedAt = c.CreatedAt
            }).ToList();
        }

        // ========= LIST DELETED (RECYCLE BIN) =========
        public async Task<List<ClassSectionListItemViewModel>> GetDeletedSectionsAsync()
        {
            var sections = await _context.ClassSections
                .Where(c => c.IsDeleted)
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                    .ThenInclude(f => f.User)
                .ToListAsync();

            return sections.Select(c => new ClassSectionListItemViewModel
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                CourseName = c.Course?.Name ?? "",
                TeacherName = c.Teacher != null ? c.Teacher.User.FullName : "",
                CreatedAt = c.CreatedAt
            }).ToList();
        }

        // ========= BUILD CREATE VM =========
        public async Task<ClassSectionViewModel> BuildCreateViewModelAsync()
        {
            var vm = new ClassSectionViewModel
            {
                Courses = await _context.Courses
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => c.Name)
                    .ToListAsync(),

                Teachers = await _context.Faculties
                    .Where(f => !f.IsDeleted && !f.User.IsDeleted)
                    .Include(f => f.User)
                    .OrderBy(f => f.User.FullName)
                    .ToListAsync()
            };

            return vm;
        }

        // ========= CREATE =========
        public async Task<(bool Success, IEnumerable<string> Errors)> CreateAsync(ClassSectionViewModel model)
        {
            var errors = new List<string>();

            // Nếu muốn thêm validate nâng cao thì nhét vào đây
            // Ví dụ: check trùng code
            var codeExists = await _context.ClassSections
                .AnyAsync(c => !c.IsDeleted && c.Code == model.Code);

            if (codeExists)
            {
                errors.Add("Class code already exists.");
                return (false, errors);
            }

            var entity = new ClassSection
            {
                Code = model.Code,
                Name = model.Name,
                CourseId = model.CourseId,
                TeacherId = model.TeacherId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.ClassSections.Add(entity);
            await _context.SaveChangesAsync();

            return (true, Enumerable.Empty<string>());
        }

        // ========= BUILD EDIT VM =========
        public async Task<ClassSectionViewModel?> BuildEditViewModelAsync(int id)
        {
            var entity = await _context.ClassSections.FindAsync(id);
            if (entity == null) return null;

            var vm = new ClassSectionViewModel
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                CourseId = entity.CourseId,
                TeacherId = entity.TeacherId,

                Courses = await _context.Courses
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => c.Name)
                    .ToListAsync(),

                Teachers = await _context.Faculties
                    .Where(f => !f.IsDeleted && !f.User.IsDeleted)
                    .Include(f => f.User)
                    .OrderBy(f => f.User.FullName)
                    .ToListAsync()
            };

            return vm;
        }

        // ========= UPDATE =========
        public async Task<(bool Success, IEnumerable<string> Errors)> UpdateAsync(ClassSectionViewModel model)
        {
            var errors = new List<string>();

            var entity = await _context.ClassSections.FindAsync(model.Id);
            if (entity == null)
            {
                errors.Add("Class section not found.");
                return (false, errors);
            }

            // Ví dụ validate trùng code (ngoại trừ chính nó)
            var codeExists = await _context.ClassSections
                .AnyAsync(c => !c.IsDeleted &&
                               c.Code == model.Code &&
                               c.Id != model.Id);

            if (codeExists)
            {
                errors.Add("Class code already exists.");
                return (false, errors);
            }

            entity.Code = model.Code;
            entity.Name = model.Name;
            entity.CourseId = model.CourseId;
            entity.TeacherId = model.TeacherId;

            await _context.SaveChangesAsync();

            return (true, Enumerable.Empty<string>());
        }

        // ========= SOFT DELETE =========
        public async Task<bool> SoftDeleteAsync(int id)
        {
            var entity = await _context.ClassSections.FindAsync(id);
            if (entity == null) return false;

            entity.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        // ========= RESTORE =========
        public async Task<bool> RestoreAsync(int id)
        {
            var entity = await _context.ClassSections.FindAsync(id);
            if (entity == null) return false;

            entity.IsDeleted = false;
            await _context.SaveChangesAsync();
            return true;
        }

        // ========= HARD DELETE =========
        public async Task<bool> HardDeleteAsync(int id)
        {
            var entity = await _context.ClassSections.FindAsync(id);
            if (entity == null) return false;

            _context.ClassSections.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
