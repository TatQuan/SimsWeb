namespace SimsWeb.ViewModels
{
    public class AssignmentDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime DueAt { get; set; }

        public string ClassCode { get; set; }
        public string? ClassName { get; set; }
        public string CourseName { get; set; }
        public string TeacherName { get; set; }

        public bool IsPastDeadline => DateTime.UtcNow > DueAt;

        // === FILE GIÁO VIÊN UP ===
        public string? ExerciseFilePath { get; set; }
        public string? GuideFilePath { get; set; }

        // === DÙNG CHO STUDENT ===
        public AssignmentSubmissionViewModel? MySubmission { get; set; }

        // === DÙNG CHO FACULTY ===
        public List<AssignmentSubmissionViewModel> Submissions { get; set; } = new();
    }

    public class AssignmentSubmissionViewModel
    {
        public int SubmissionId { get; set; }
        public int AssignmentId { get; set; }
        public int StudentId { get; set; }
        public string? StudentCode { get; set; }
        public string? StudentName { get; set; }

        public DateTime? SubmittedAt { get; set; }

        // FILE STUDENT NỘP
        public string? FilePath { get; set; }

        public int? Score { get; set; }
        public string? TeacherComment { get; set; }
        public bool IsLate { get; set; }
    }
}
