using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SimsWeb.Models;

namespace SimsWeb.ViewModels
{
    public class ClassScheduleViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Class Section")]
        public int ClassSectionId { get; set; }

        [Required]
        [Display(Name = "Day of Week")]
        public DayOfWeek DayOfWeek { get; set; }

        // Tiết: 1..8
        [Required]
        [Display(Name = "Period")]
        public int Period { get; set; }

        // Vẫn giữ lại để map vào DB, nhưng user không nhập trực tiếp
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public string? Room { get; set; }

        public List<ClassSection> ClassSections { get; set; } = new();
    }
}
