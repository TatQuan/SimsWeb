using System;

namespace SimsWeb.ViewModels
{
    public class AdminProfileViewModel
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime AccountCreatedAt { get; set; }

        public int TotalStudents { get; set; }
        public int TotalFaculty { get; set; }
        public int TotalCourses { get; set; }
        public int TotalActiveClasses { get; set; }
        public int TodayScheduleCount { get; set; }
    }
}
