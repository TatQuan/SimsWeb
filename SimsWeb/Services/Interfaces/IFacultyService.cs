using SimsWeb.ViewModels;

namespace SimsWeb.Services.Interfaces
{
    public interface IFacultyService
    {
        Task<List<FacultyListItemViewModel>> GetActiveAsync();
        Task<List<FacultyListItemViewModel>> GetDeletedAsync();

        Task<FacultyEditViewModel> BuildCreateViewModelAsync();
        Task<(bool Success, IEnumerable<string> Errors)> CreateAsync(FacultyEditViewModel model);

        Task<FacultyEditViewModel?> BuildEditViewModelAsync(int id);
        Task<(bool Success, IEnumerable<string> Errors)> UpdateAsync(FacultyEditViewModel model);

        Task<bool> SoftDeleteAsync(int id);
        Task<bool> RestoreAsync(int id);
        Task<bool> HardDeleteAsync(int id);
    }
}
