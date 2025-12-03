using Microsoft.AspNetCore.Http;
using SimsWeb.ViewModels;

namespace SimsWeb.Services.Interfaces
{
    public interface IStudentAssignmentService
    {
        Task<List<AssignmentListItemViewModel>> GetAssignmentsForStudentAsync(string studentUserId);
        Task<AssignmentDetailViewModel?> GetAssignmentDetailForStudentAsync(int assignmentId, string studentUserId);
        Task<bool> SubmitAsync(int assignmentId, string studentUserId, IFormFile file);
    }
}
