using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimsWeb.Models;
using SimsWeb.Services.Interfaces;
using SimsWeb.ViewModels;

namespace SimsWeb.Services.Implementations
{
    public class AdminUserService : IAdminUserService
    {
        private readonly UserManager<Users> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public AdminUserService(
            UserManager<Users> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        // ============ LIST ACTIVE USERS ============
        public async Task<List<UserListItemViewModel>> GetActiveUsersAsync()
        {
            var users = await userManager.Users
                .Where(u => !u.IsDeleted)
                .ToListAsync();

            var model = new List<UserListItemViewModel>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);

                model.Add(new UserListItemViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    UserName = user.UserName,
                    Roles = roles,
                    CreatedAt = user.CreatedAt
                });
            }

            return model;
        }

        // ============ LIST DELETED USERS ============
        public async Task<List<UserListItemViewModel>> GetDeletedUsersAsync()
        {
            var users = await userManager.Users
                .Where(u => u.IsDeleted)
                .ToListAsync();

            var model = new List<UserListItemViewModel>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);

                model.Add(new UserListItemViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    UserName = user.UserName,
                    Roles = roles,
                    CreatedAt = user.CreatedAt
                });
            }

            return model;
        }

        // ============ CREATE (GET) ============
        public async Task<AdminCreateUserViewModel> GetCreateViewModelAsync()
        {
            var roles = await roleManager.Roles
                .Select(r => r.Name!)
                .ToListAsync();

            return new AdminCreateUserViewModel
            {
                RoleList = roles
            };
        }

        // ============ CREATE (POST) ============
        public async Task<(bool Success, IEnumerable<string> Errors)> CreateUserAsync(AdminCreateUserViewModel model)
        {
            var user = new Users
            {
                FullName = model.Name,
                Email = model.Email,
                UserName = model.Email,
                NormalizedEmail = model.Email.ToUpper(),
                NormalizedUserName = model.Email.ToUpper(),
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return (false, result.Errors.Select(e => e.Description));
            }

            // Nếu admin không chọn role → mặc định Guest
            var roleToAssign = string.IsNullOrEmpty(model.SelectedRole)
                ? "Guest"
                : model.SelectedRole;

            // Đảm bảo role này tồn tại
            if (!await roleManager.RoleExistsAsync(roleToAssign))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(roleToAssign));
                if (!roleResult.Succeeded)
                {
                    // nếu tạo role fail → trả lỗi
                    return (false, roleResult.Errors.Select(e => e.Description));
                }
            }

            await userManager.AddToRoleAsync(user, roleToAssign);

            return (true, Enumerable.Empty<string>());
        }

        // ============ EDIT (GET) ============
        public async Task<UserEditViewModel?> GetEditViewModelAsync(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null) return null;

            var roles = await userManager.GetRolesAsync(user);

            var vm = new UserEditViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                SelectedRole = roles.FirstOrDefault(),
                RoleList = await roleManager.Roles.Select(r => r.Name!).ToListAsync()
            };

            return vm;
        }

        // ============ EDIT (POST) ============
        public async Task<(bool Success, IEnumerable<string> Errors)> UpdateUserAsync(UserEditViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return (false, new[] { "User not found." });
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.NormalizedEmail = model.Email.ToUpper();
            user.NormalizedUserName = model.Email.ToUpper();

            var updateResult = await userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return (false, updateResult.Errors.Select(e => e.Description));
            }

            // Cập nhật role
            var currentRoles = await userManager.GetRolesAsync(user);

            if (currentRoles.Any())
                await userManager.RemoveFromRolesAsync(user, currentRoles);

            var roleToAssign = string.IsNullOrEmpty(model.SelectedRole)
                ? "Guest"
                : model.SelectedRole;

            // Đảm bảo role tồn tại
            if (!await roleManager.RoleExistsAsync(roleToAssign))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(roleToAssign));
                if (!roleResult.Succeeded)
                {
                    return (false, roleResult.Errors.Select(e => e.Description));
                }
            }

            await userManager.AddToRoleAsync(user, roleToAssign);

            return (true, Enumerable.Empty<string>());
        }

        // ============ SOFT DELETE ============
        public async Task<bool> SoftDeleteAsync(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null) return false;

            user.IsDeleted = true;
            var result = await userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        // ============ RESTORE ============
        public async Task<bool> RestoreAsync(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null) return false;

            user.IsDeleted = false;
            var result = await userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        // ============ HARD DELETE ============
        public async Task<bool> HardDeleteAsync(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null) return false;

            var result = await userManager.DeleteAsync(user);
            return result.Succeeded;
        }


    }
}
