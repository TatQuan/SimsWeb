using System.ComponentModel.DataAnnotations;

namespace SimsWeb.Models
{
    public class AssignmentSubmission
    {
        public int Id { get; set; }

        [Required]
        public int AssignmentId { get; set; }
        public Assignment Assignment { get; set; }

        [Required]
        public int StudentId { get; set; }
        public Student Student { get; set; }

        // Thời điểm nộp
        public DateTime SubmittedAt { get; set; }

        // File lưu trong wwwroot/uploads/asm/... (tạm dùng string path)
        [Required, StringLength(500)]
        public string FilePath { get; set; }

        // Điểm & nhận xét (nullable – chưa chấm thì null)
        public int? Score { get; set; }
        public DateTime? GradedAt { get; set; }

        [StringLength(2000)]
        public string? TeacherComment { get; set; }

        public bool IsDeleted { get; set; } = false;

        // (Optional) cho view: đã chấm hay chưa
        public bool IsGraded => Score.HasValue;
    }
}
