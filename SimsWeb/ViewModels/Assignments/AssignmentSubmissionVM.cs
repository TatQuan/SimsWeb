namespace SimsWeb.ViewModels.Assignments
{
    public class AssignmentSubmissionVM
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string DownloadUrl { get; set; } = null!;
        public DateTime SubmittedAt { get; set; }

        public int? Score { get; set; }
        public string? TeacherComment { get; set; }
    }
}
