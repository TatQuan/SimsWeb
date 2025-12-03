using System.ComponentModel.DataAnnotations;

namespace SimsWeb.Models
{
    public class AssignmentResource
    {
        public int Id { get; set; }

        [Required]
        public int AssignmentId { get; set; }
        public Assignment Assignment { get; set; }

        [Required, StringLength(255)]
        public string FileName { get; set; }  // tên file hiển thị cho user

        [Required, StringLength(500)]
        public string FilePath { get; set; }  // path trong wwwroot / storage

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
