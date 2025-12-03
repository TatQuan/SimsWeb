using Microsoft.AspNetCore.Http;
using SimsWeb.Models;
using System.ComponentModel.DataAnnotations;

namespace SimsWeb.ViewModels
{
    public class AssignmentCreateViewModel
    {
        [Required]
        public int ClassSectionId { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Required, DataType(DataType.Time)]
        public TimeSpan DueTime { get; set; }

        public int MaxScore { get; set; } = 100;

        public List<ClassSection> TeacherClassSections { get; set; } = new();

        // NEW: file bài tập + file hướng dẫn
        public IFormFile? ExerciseFile { get; set; }
        public IFormFile? GuideFile { get; set; }
    }
}
