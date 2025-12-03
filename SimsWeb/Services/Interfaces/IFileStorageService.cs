using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SimsWeb.Services.Interfaces
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Lưu file tài liệu/brief/guide của assignment (phía teacher)
        /// </summary>
        /// <param name="assignmentId">Id của Assignment</param>
        /// <param name="file">File upload từ form</param>
        /// <returns>Đường dẫn public (ví dụ: /uploads/assignments/1/xxx.pdf)</returns>
        Task<string> SaveAssignmentFileAsync(int assignmentId, IFormFile file);

        /// <summary>
        /// Lưu file submission của student
        /// </summary>
        /// <param name="assignmentId">Id của Assignment</param>
        /// <param name="studentId">Id của Student</param>
        /// <param name="file">File upload từ form</param>
        /// <returns>Đường dẫn public (ví dụ: /uploads/submissions/1/10/xxx.pdf)</returns>
        Task<string> SaveSubmissionFileAsync(int assignmentId, int studentId, IFormFile file);

        /// <summary>
        /// Trả về URL public để client dùng trong &lt;a href&gt;.
        /// Nếu cậu lưu kiểu /uploads/... thì có thể đơn giản chỉ return y nguyên.
        /// </summary>
        string GetPublicUrl(string filePath);

        /// <summary>
        /// Xoá file khỏi storage (optional, dùng khi cần)
        /// </summary>
        Task DeleteFileAsync(string filePath);
    }
}
