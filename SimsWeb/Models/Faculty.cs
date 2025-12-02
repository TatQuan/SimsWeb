using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimsWeb.Models
{
    public class Faculty
    {
        public int Id { get; set; }

        // Liên kết 1-1 với Users (tài khoản đăng nhập)
        [Required]
        public string UserId { get; set; }
        public Users User { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }   // Bộ môn / Khoa

        [StringLength(100)]
        public string? Title { get; set; }        // Lecturer, Professor,...

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

        // Mã giảng viên tự sinh: FAC0001, FAC0002,...
        [NotMapped]
        public string FacultyCode => $"FAC{Id.ToString().PadLeft(4, '0')}";
    }
}
