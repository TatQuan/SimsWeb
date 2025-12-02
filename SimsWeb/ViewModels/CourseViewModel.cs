using System.ComponentModel.DataAnnotations;

namespace SimsWeb.ViewModels
{
    public class CourseViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Course Code")]
        public string Code { get; set; }

        [Required]
        [Display(Name = "Course Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Credits")]
        public int? Credits { get; set; }
    }
}
