using Microsoft.AspNetCore.Http;
using SimsWeb.Models;

namespace SimsWeb.ViewModels
{
    // Kế thừa lại Create cho đỡ lặp
    public class AssignmentEditViewModel : AssignmentCreateViewModel
    {
        public int Id { get; set; }

        // Đường dẫn file hiện tại để hiển thị link Download
        public string? ExistingExerciseFilePath { get; set; }
        public string? ExistingGuideFilePath { get; set; }
    }
}
