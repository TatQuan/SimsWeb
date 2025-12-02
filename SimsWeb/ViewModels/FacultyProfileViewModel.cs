using System;
using System.Collections.Generic;

namespace SimsWeb.ViewModels
{
    public class FacultyProfileViewModel
    {
        // User
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public DateTime AccountCreatedAt { get; set; }

        // Faculty
        public string FacultyCode { get; set; } = "";
        public string? Department { get; set; }
        public string? Title { get; set; }
        public DateTime? FacultyCreatedAt { get; set; }

        // Summary
        public int CurrentClassCount { get; set; }
        public int TotalDistinctCourses { get; set; }
        public int TotalCredits { get; set; }

        public List<FacultyClassItemViewModel> CurrentClasses { get; set; } = new();
        public List<ScheduleItemViewModel> TodaySchedule { get; set; } = new();
    }

    public class FacultyClassItemViewModel
    {
        public int ClassSectionId { get; set; }
        public string ClassCode { get; set; } = "";
        public string? ClassName { get; set; }
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public int Credits { get; set; }
        public int StudentCount { get; set; }
    }
}
