namespace SimsWeb.ViewModels
{
    public class ScheduleItemViewModel
    {
        public int ClassSectionId { get; set; }
        public string ClassCode { get; set; }
        public string? ClassName { get; set; }

        public string CourseName { get; set; }
        public string TeacherName { get; set; }

        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Room { get; set; }
    }
}
