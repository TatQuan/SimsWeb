using Microsoft.AspNetCore.Http;

namespace SimsWeb.Services.Interfaces
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Lưu file bài nộp assignment và trả về đường dẫn (relative) để lưu trong DB.
        /// </summary>
        Task<string> SaveAssignmentFileAsync(int assignmentId, int studentId, IFormFile file);
        Task<string> SaveAssignmentMaterialAsync(int assignmentId, IFormFile file, string kind);
        // kind: "exercise" hoặc "guide"
    }
}
