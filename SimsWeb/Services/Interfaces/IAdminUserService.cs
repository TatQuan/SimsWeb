using SimsWeb.ViewModels;

namespace SimsWeb.Services.Interfaces
{
    public interface IAdminUserService
    {
        Task<List<UserListItemViewModel>> GetActiveUsersAsync();
        Task<List<UserListItemViewModel>> GetDeletedUsersAsync();

        Task<AdminCreateUserViewModel> GetCreateViewModelAsync();
        Task<(bool Success, IEnumerable<string> Errors)> CreateUserAsync(AdminCreateUserViewModel model);

        Task<UserEditViewModel?> GetEditViewModelAsync(string id);
        Task<(bool Success, IEnumerable<string> Errors)> UpdateUserAsync(UserEditViewModel model);

        Task<bool> SoftDeleteAsync(string id);
        Task<bool> RestoreAsync(string id);
        Task<bool> HardDeleteAsync(string id);
    }
}
