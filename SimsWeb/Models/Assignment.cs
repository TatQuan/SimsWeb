using System.ComponentModel.DataAnnotations;

namespace SimsWeb.Models
{
    public class Assignment
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        // Mỗi ASM thuộc 1 ClassSection
        [Required]
        public int ClassSectionId { get; set; }
        public ClassSection ClassSection { get; set; }

        // Deadline: cả ngày + giờ
        [Required]
        public DateTime DueAt { get; set; }

        // Điểm tối đa
        public int MaxScore { get; set; } = 100;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

        // === Assignment Brief (file đề bài chính) ===
        // ex: /uploads/assignments/1_brief.pdf
        public string? ExerciseFilePath { get; set; }

        // === File hướng dẫn/guide (cũng có thể show trong Document) ===
        // ex: /uploads/assignments/1_guide.pdf
        public string? GuideFilePath { get; set; }

        // === Danh sách Document (tài liệu bổ sung) ===
        public ICollection<AssignmentResource> Resources { get; set; }
            = new List<AssignmentResource>();

        // Danh sách submissions của sinh viên
        public ICollection<AssignmentSubmission> Submissions { get; set; }
            = new List<AssignmentSubmission>();
    }
}
