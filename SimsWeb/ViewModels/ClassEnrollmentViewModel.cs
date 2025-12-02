using System.Collections.Generic;

namespace SimsWeb.ViewModels
{
    public class ClassEnrollmentViewModel
    {
        public int ClassSectionId { get; set; }
        public string ClassCode { get; set; }
        public string? ClassName { get; set; }
        public string CourseName { get; set; }
        public string TeacherName { get; set; }

        public List<ClassEnrollmentStudentItemViewModel> EnrolledStudents { get; set; }
            = new();

        public List<ClassEnrollmentStudentItemViewModel> AvailableStudents { get; set; }
            = new();

        // các StudentId được chọn bên form (multi select)
        public int[] SelectedStudentIds { get; set; } = new int[0];
    }
}
