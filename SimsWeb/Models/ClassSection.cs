using System.ComponentModel.DataAnnotations;

namespace SimsWeb.Models
{
    public class ClassSection
    {
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string Code { get; set; }  // VD: CS101-A

        [StringLength(100)]
        public string? Name { get; set; }

        // Khóa ngoại Course
        [Required]
        public int CourseId { get; set; }
        public Course Course { get; set; }

        // Khóa ngoại Teacher (User có role Faculty)
        [Required]
        public int TeacherId { get; set; }
        public Faculty Teacher { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}

