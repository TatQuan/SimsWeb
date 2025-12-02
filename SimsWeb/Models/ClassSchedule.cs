using System;
using System.ComponentModel.DataAnnotations;

namespace SimsWeb.Models
{
    public class ClassSchedule
    {
        public int Id { get; set; }

        [Required]
        public int ClassSectionId { get; set; }
        public ClassSection ClassSection { get; set; }

        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [StringLength(50)]
        public string? Room { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
