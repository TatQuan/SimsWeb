namespace SimsWeb.ViewModels.Assignments
{
    public class UploadAssignmentFileVM
    {
        public int AssignmentId { get; set; }
        public string UploadType { get; set; } = null!; // "brief", "guide", "document"
        public IFormFile File { get; set; } = null!;
    }

    public class SubmitAssignmentVM
    {
        public int AssignmentId { get; set; }
        public IFormFile File { get; set; } = null!;
    }
}
