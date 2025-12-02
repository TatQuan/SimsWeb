using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SimsWeb.Models;

namespace SimsWeb.ViewModels
{
    public class FacultyEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Linked User (Faculty)")]
        public string UserId { get; set; }

        [Display(Name = "Department")]
        public string? Department { get; set; }

        [Display(Name = "Title")]
        public string? Title { get; set; }

        // Hiển thị mã giảng viên khi Edit
        public string? DisplayFacultyCode { get; set; }

        // Dropdown user có role Faculty
        public List<Users> FacultyUsers { get; set; } = new();
    }
}
