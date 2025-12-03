using SimsWeb.ViewModels;

namespace SimsWeb.Services.Interfaces
{
    public interface ITeacherAssignmentService
    {
        Task<List<AssignmentListItemViewModel>> GetAssignmentsForTeacherAsync(string teacherUserId);
        Task<AssignmentCreateViewModel> BuildCreateViewModelAsync(string teacherUserId);
        Task<int?> CreateAsync(AssignmentCreateViewModel model, string teacherUserId);
        Task<AssignmentDetailViewModel?> GetAssignmentDetailForTeacherAsync(int assignmentId, string teacherUserId);
        Task<bool> GradeSubmissionAsync(int submissionId, int score, string? comment);

        Task<AssignmentEditViewModel?> BuildEditViewModelAsync(int id, string teacherUserId);
        Task<bool> UpdateAsync(AssignmentEditViewModel model, string teacherUserId);
        Task<bool> SoftDeleteAsync(int id, string teacherUserId);
    }
}
