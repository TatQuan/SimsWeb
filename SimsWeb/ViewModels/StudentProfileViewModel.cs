using System;
using System.Collections.Generic;

namespace SimsWeb.ViewModels
{
    public class StudentProfileViewModel
    {
        // User
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public DateTime AccountCreatedAt { get; set; }

        // Student
        public string StudentCode { get; set; } = "";
        public DateTime? StudentCreatedAt { get; set; }

        // Summary
        public int CurrentClassCount { get; set; }
        public int TotalCredits { get; set; }

        public List<StudentClassItemViewModel> CurrentClasses { get; set; } = new();
        public List<ScheduleItemViewModel> TodaySchedule { get; set; } = new();
    }

    public class StudentClassItemViewModel
    {
        public int ClassSectionId { get; set; }
        public string ClassCode { get; set; } = "";
        public string? ClassName { get; set; }
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public int Credits { get; set; }
        public string TeacherName { get; set; } = "";
        public DateTime EnrolledAt { get; set; }
    }
}
