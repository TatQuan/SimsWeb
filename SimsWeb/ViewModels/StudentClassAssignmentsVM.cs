namespace SimsWeb.ViewModels.Assignments
{
    public class StudentClassAssignmentsVM
    {
        public int ClassSectionId { get; set; }
        public string ClassName { get; set; } = null!;
        public string CourseName { get; set; } = null!;

        public List<AssignmentListItemVM> Assignments { get; set; } = new();
    }
}
