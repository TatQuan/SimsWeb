using System.ComponentModel.DataAnnotations;
using SimsWeb.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;   // THÊM USING NÀY

namespace SimsWeb.ViewModels
{
    public class ClassSectionViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Class Code")]
        public string Code { get; set; }

        [Display(Name = "Class Name")]
        public string? Name { get; set; }

        [Required]
        [Display(Name = "Course")]
        public int CourseId { get; set; }

        [Required]
        [Display(Name = "Teacher")]
        public int TeacherId { get; set; }

        // Chỉ để hiện dropdown → KHÔNG cần validate
        [ValidateNever]
        public List<Course> Courses { get; set; } = new();

        [ValidateNever]
        public List<Faculty> Teachers { get; set; } = new();
    }
}
