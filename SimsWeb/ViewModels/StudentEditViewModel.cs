using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SimsWeb.Models;

namespace SimsWeb.ViewModels
{
    public class StudentEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Linked User (Student)")]
        public string UserId { get; set; }

        // Hiển thị mã sinh viên trong Edit
        public string? DisplayStudentCode { get; set; }

        // Cho dropdown
        public List<Users> StudentUsers { get; set; } = new();
        public List<ClassSection> ClassSections { get; set; } = new();
    }
}
