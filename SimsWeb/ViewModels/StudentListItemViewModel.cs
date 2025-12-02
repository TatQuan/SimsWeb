using System;

namespace SimsWeb.ViewModels
{
    public class StudentListItemViewModel
    {
        public int Id { get; set; }
        public string StudentCode { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
