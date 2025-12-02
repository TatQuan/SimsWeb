using SimsWeb.ViewModels;

namespace SimsWeb.Services.Interfaces
{
    public interface IStudentService
    {
        Task<List<StudentListItemViewModel>> GetActiveAsync();
        Task<List<StudentListItemViewModel>> GetDeletedAsync();

        Task<StudentEditViewModel> BuildCreateViewModelAsync();
        Task<(bool Success, IEnumerable<string> Errors)> CreateAsync(StudentEditViewModel model);

        Task<StudentEditViewModel?> BuildEditViewModelAsync(int id);
        Task<(bool Success, IEnumerable<string> Errors)> UpdateAsync(StudentEditViewModel model);

        Task<bool> SoftDeleteAsync(int id);
        Task<bool> RestoreAsync(int id);
        Task<bool> HardDeleteAsync(int id);
    }
}
