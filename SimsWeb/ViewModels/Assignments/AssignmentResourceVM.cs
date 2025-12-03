namespace SimsWeb.ViewModels.Assignments
{
    public class AssignmentResourceVM
    {
        public int Id { get; set; }
        public string FileName { get; set; } = null!;
        public string DownloadUrl { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
    }
}
