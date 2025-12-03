namespace SimsWeb.ViewModels.Assignments
{
    public class AssignmentEditVM
    {
        public int? Id { get; set; } // null = create
        public int ClassSectionId { get; set; }

        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime DueAt { get; set; }
        public int MaxScore { get; set; } = 100;
    }
}
