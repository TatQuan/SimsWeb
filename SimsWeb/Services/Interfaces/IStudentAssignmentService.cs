using SimsWeb.ViewModels.Assignments;

public interface IStudentAssignmentService
{
    Task<List<ClassSectionCardVM>> GetStudentClassesAsync(int studentId);
    Task<AssignmentDetailVM> GetAssignmentDetailAsync(int assignmentId, int studentId);

    Task SubmitAsync(SubmitAssignmentVM model, int studentId);
}
