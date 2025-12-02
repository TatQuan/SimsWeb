using System.ComponentModel.DataAnnotations;

namespace SimsWeb.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(250)]
        public string? Description { get; set; }

        public int? Credits { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // XÓA MỀM
        public bool IsDeleted { get; set; } = false;
    }
}
