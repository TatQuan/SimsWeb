using SimsWeb.ViewModels;
using SimsWeb.Models;

namespace SimsWeb.Services.Interfaces
{
    public interface IScheduleService
    {
        // Lịch cho gv / sv (xem)
        Task<List<ScheduleItemViewModel>> GetScheduleForTeacherAsync(string userId);
        Task<List<ScheduleItemViewModel>> GetScheduleForStudentAsync(string userId);

        // CRUD + recycle bin
        Task<List<ClassSchedule>> GetActiveSchedulesAsync();
        Task<List<ClassSchedule>> GetDeletedSchedulesAsync();

        Task<ClassScheduleViewModel?> BuildCreateViewModelAsync();
        Task<ClassScheduleViewModel?> BuildEditViewModelAsync(int id);

        Task<bool> CreateAsync(ClassScheduleViewModel model);
        Task<bool> UpdateAsync(ClassScheduleViewModel model);

        Task<bool> SoftDeleteAsync(int id);
        Task<bool> RestoreAsync(int id);
        Task<bool> HardDeleteAsync(int id);

    }
}
