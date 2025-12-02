using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimsWeb.Data;
using SimsWeb.Models;
using SimsWeb.Services.Interfaces;
using SimsWeb.ViewModels;

namespace SimsWeb.Services.Implementations
{
    public class FacultyService : IFacultyService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public FacultyService(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ============= LIST ACTIVE =============
        public async Task<List<FacultyListItemViewModel>> GetActiveAsync()
        {
            var faculties = await _context.Faculties
                .Where(f => !f.IsDeleted)
                .Include(f => f.User)
                .ToListAsync();

            return faculties.Select(f => new FacultyListItemViewModel
            {
                Id = f.Id,
                FacultyCode = f.FacultyCode,
                FullName = f.User.FullName,
                Email = f.User.Email,
                Department = f.Department,
                Title = f.Title,
                CreatedAt = f.CreatedAt
            }).ToList();
        }

        // ============= LIST DELETED (RECYCLE BIN) =============
        public async Task<List<FacultyListItemViewModel>> GetDeletedAsync()
        {
            var faculties = await _context.Faculties
                .Where(f => f.IsDeleted)
                .Include(f => f.User)
                .ToListAsync();

            return faculties.Select(f => new FacultyListItemViewModel
            {
                Id = f.Id,
                FacultyCode = f.FacultyCode,
                FullName = f.User.FullName,
                Email = f.User.Email,
                Department = f.Department,
                Title = f.Title,
                CreatedAt = f.CreatedAt
            }).ToList();
        }

        // helper load dropdown user cho FacultyEditViewModel
        private async Task LoadFacultyUsersAsync(FacultyEditViewModel model)
        {
            // lấy user trong role Faculty
            var facultyUsers = await _userManager.GetUsersInRoleAsync("Faculty");

            // nếu muốn loại user đã có faculty profile:
            var existingIds = await _context.Faculties
                .Where(f => !f.IsDeleted)
                .Select(f => f.UserId)
                .ToListAsync();

            var list = facultyUsers.Where(u => !existingIds.Contains(u.Id) || u.Id == model.UserId);

            //var list = facultyUsers
            //    .Where(u => !u.IsDeleted)
            //    .ToList();

            //model.FacultyUsers = list;
        }

        // ============= BUILD CREATE VM =============
        public async Task<FacultyEditViewModel> BuildCreateViewModelAsync()
        {
            var vm = new FacultyEditViewModel();
            await LoadFacultyUsersAsync(vm);
            return vm;
        }

        // ============= CREATE =============
        public async Task<(bool Success, IEnumerable<string> Errors)> CreateAsync(FacultyEditViewModel model)
        {
            var errors = new List<string>();

            // check user tồn tại
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                errors.Add("Selected user not found.");
                return (false, errors);
            }

            // check user có role Faculty
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Faculty"))
            {
                errors.Add("Selected user is not in Faculty role.");
                return (false, errors);
            }

            // check user đã có faculty profile chưa
            var existed = await _context.Faculties
                .AnyAsync(f => f.UserId == model.UserId && !f.IsDeleted);

            if (existed)
            {
                errors.Add("This user already has a faculty profile.");
                return (false, errors);
            }

            var entity = new Faculty
            {
                UserId = model.UserId,
                Department = model.Department,
                Title = model.Title,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Faculties.Add(entity);
            await _context.SaveChangesAsync();

            return (true, Enumerable.Empty<string>());
        }

        // ============= BUILD EDIT VM =============
        public async Task<FacultyEditViewModel?> BuildEditViewModelAsync(int id)
        {
            var entity = await _context.Faculties
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (entity == null) return null;

            var vm = new FacultyEditViewModel
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Department = entity.Department,
                Title = entity.Title,
                DisplayFacultyCode = entity.FacultyCode
            };

            await LoadFacultyUsersAsync(vm);
            return vm;
        }

        // ============= UPDATE =============
        public async Task<(bool Success, IEnumerable<string> Errors)> UpdateAsync(FacultyEditViewModel model)
        {
            var errors = new List<string>();

            var entity = await _context.Faculties.FindAsync(model.Id);
            if (entity == null)
            {
                errors.Add("Faculty profile not found.");
                return (false, errors);
            }

            // check user tồn tại
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                errors.Add("Selected user not found.");
                return (false, errors);
            }

            // check role
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Faculty"))
            {
                errors.Add("Selected user is not in Faculty role.");
                return (false, errors);
            }

            // check không trùng faculty khác
            var existed = await _context.Faculties
                .AnyAsync(f => f.UserId == model.UserId &&
                               f.Id != model.Id &&
                               !f.IsDeleted);

            if (existed)
            {
                errors.Add("This user already has another faculty profile.");
                return (false, errors);
            }

            entity.UserId = model.UserId;
            entity.Department = model.Department;
            entity.Title = model.Title;

            _context.Faculties.Update(entity);
            await _context.SaveChangesAsync();

            return (true, Enumerable.Empty<string>());
        }

        // ============= SOFT DELETE =============
        public async Task<bool> SoftDeleteAsync(int id)
        {
            var entity = await _context.Faculties.FindAsync(id);
            if (entity == null) return false;

            entity.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        // ============= RESTORE =============
        public async Task<bool> RestoreAsync(int id)
        {
            var entity = await _context.Faculties.FindAsync(id);
            if (entity == null) return false;

            entity.IsDeleted = false;
            await _context.SaveChangesAsync();
            return true;
        }

        // ============= HARD DELETE =============
        public async Task<bool> HardDeleteAsync(int id)
        {
            var entity = await _context.Faculties.FindAsync(id);
            if (entity == null) return false;

            _context.Faculties.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
