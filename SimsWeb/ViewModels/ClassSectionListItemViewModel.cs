namespace SimsWeb.ViewModels
{
    public class ClassSectionListItemViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string? Name { get; set; }

        public string CourseName { get; set; }
        public string TeacherName { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
