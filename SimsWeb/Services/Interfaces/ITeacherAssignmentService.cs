using SimsWeb.ViewModels.Assignments;

public interface ITeacherAssignmentService
{
    Task<List<ClassSectionCardVM>> GetTeacherClassesAsync(int facultyId);

    // trang quản lý assignment theo lớp
    Task<ClassAssignmentPageVM> GetClassAssignmentPageAsync(int facultyId, int classSectionId, int? assignmentId);

    Task<int> SaveAssignmentAsync(int facultyId, AssignmentEditVM model); // create / update
    Task SoftDeleteAssignmentAsync(int facultyId, int assignmentId);
    Task RestoreAssignmentAsync(int facultyId, int assignmentId);
    Task HardDeleteAssignmentAsync(int facultyId, int assignmentId);
}
