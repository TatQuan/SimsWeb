namespace SimsWeb.ViewModels.Assignments
{
    public class ClassAssignmentPageVM
    {
        // Thông tin lớp
        public int ClassSectionId { get; set; }
        public string ClassName { get; set; } = null!;
        public string CourseName { get; set; } = null!;

        // List assignment
        public List<AssignmentListItemVM> ActiveAssignments { get; set; } = new();
        public List<AssignmentListItemVM> DeletedAssignments { get; set; } = new();

        // Form create / edit
        public AssignmentEditVM EditModel { get; set; } = new AssignmentEditVM();
    }
}
