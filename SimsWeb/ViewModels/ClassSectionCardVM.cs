namespace SimsWeb.ViewModels.Assignments
{
    public class ClassSectionCardVM
    {
        public int ClassSectionId { get; set; }
        public string ClassName { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public int StudentCount { get; set; }
    }
}
