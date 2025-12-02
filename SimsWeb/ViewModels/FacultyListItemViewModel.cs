using System;

namespace SimsWeb.ViewModels
{
    public class FacultyListItemViewModel
    {
        public int Id { get; set; }
        public string FacultyCode { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }

        public string? Department { get; set; }
        public string? Title { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
