using SimsWeb.Models;
using SimsWeb.ViewModels;

namespace SimsWeb.Services.Interfaces
{
    public interface IEnrollmentService
    {
        //Task<List<ClassSection>> GetActiveClassSectionsAsync();

        Task<ClassEnrollmentViewModel?> BuildManageViewModelAsync(int classSectionId);

        Task<(bool Success, string Message)> AddStudentsAsync(int classSectionId, int[] selectedStudentIds);
        Task<(bool Success, string Message)> RemoveStudentAsync(int classSectionId, int studentId);
        Task<List<ClassSection>> GetActiveClassSectionsAsync(int? courseId = null);
    }
}
