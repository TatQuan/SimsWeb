namespace SimsWeb.ViewModels
{
    public class AssignmentListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ClassCode { get; set; }
        public string? ClassName { get; set; }
        public string CourseName { get; set; }
        public DateTime DueAt { get; set; }

        // cho Student
        public bool IsSubmitted { get; set; }
        public bool IsLate { get; set; }
        public int? Score { get; set; }
    }
}
