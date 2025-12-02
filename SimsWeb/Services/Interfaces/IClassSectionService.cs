using SimsWeb.ViewModels;

namespace SimsWeb.Services.Interfaces
{
    public interface IClassSectionService
    {
        Task<List<ClassSectionListItemViewModel>> GetActiveSectionsAsync();

        Task<List<ClassSectionListItemViewModel>> GetDeletedSectionsAsync();

        Task<ClassSectionViewModel> BuildCreateViewModelAsync();
        Task<(bool Success, IEnumerable<string> Errors)> CreateAsync(ClassSectionViewModel model);

        Task<ClassSectionViewModel?> BuildEditViewModelAsync(int id);
        Task<(bool Success, IEnumerable<string> Errors)> UpdateAsync(ClassSectionViewModel model);

        Task<bool> SoftDeleteAsync(int id);
        Task<bool> RestoreAsync(int id);
        Task<bool> HardDeleteAsync(int id);
    }
}
