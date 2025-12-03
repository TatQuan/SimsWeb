using SimsWeb.Models;

namespace SimsWeb.ViewModels.Assignments
{
    public class AssignmentDetailVM
    {
        public int AssignmentId { get; set; }
        public int ClassSectionId { get; set; }

        public string ClassName { get; set; } = null!;
        public string CourseName { get; set; } = null!;

        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        public DateTime DueAt { get; set; }
        public int MaxScore { get; set; }

        // Brief
        public string? ExerciseDownloadUrl { get; set; }
        public string? GuideDownloadUrl { get; set; }

        // Tài liệu Document
        public List<AssignmentResourceVM> Documents { get; set; } = new();

        // Nếu là giáo viên → toàn bộ bài nộp
        public List<AssignmentSubmissionVM> AllSubmissions { get; set; } = new();

        // Nếu là student → bài mình nộp
        public AssignmentSubmissionVM? MySubmission { get; set; }

        // Flag để View đỡ phải query role
        public bool IsTeacherView { get; set; }
    }
}
