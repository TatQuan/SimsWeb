using System;

namespace SimsWeb.ViewModels.Assignments
{
    public class AssignmentListItemVM
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public DateTime DueAt { get; set; }

        // dùng cho recycle bin (IsDeleted = true)
        public bool IsDeleted { get; set; }
    }
}
