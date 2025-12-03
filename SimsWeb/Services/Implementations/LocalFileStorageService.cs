using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SimsWeb.Services.Interfaces;

namespace SimsWeb.Services.Implementations
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _webRootPath;

        public LocalFileStorageService(IWebHostEnvironment env)
        {
            _webRootPath = env.WebRootPath;
        }

        public async Task<string> SaveAssignmentFileAsync(int assignmentId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            // thư mục vật lý: wwwroot/uploads/assignments/{assignmentId}
            var folderRelative = $"/uploads/assignments/{assignmentId}";
            var folderPhysical = Path.Combine(_webRootPath, "uploads", "assignments", assignmentId.ToString());

            Directory.CreateDirectory(folderPhysical);

            var uniqueFileName = BuildUniqueFileName(file.FileName);
            var physicalPath = Path.Combine(folderPhysical, uniqueFileName);

            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // đường dẫn public để dùng trong href
            var publicPath = $"{folderRelative}/{uniqueFileName}";
            return publicPath.Replace("\\", "/");
        }

        public async Task<string> SaveSubmissionFileAsync(int assignmentId, int studentId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            // thư mục vật lý: wwwroot/uploads/submissions/{assignmentId}/{studentId}
            var folderRelative = $"/uploads/submissions/{assignmentId}/{studentId}";
            var folderPhysical = Path.Combine(
                _webRootPath,
                "uploads",
                "submissions",
                assignmentId.ToString(),
                studentId.ToString()
            );

            Directory.CreateDirectory(folderPhysical);

            var uniqueFileName = BuildUniqueFileName(file.FileName);
            var physicalPath = Path.Combine(folderPhysical, uniqueFileName);

            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var publicPath = $"{folderRelative}/{uniqueFileName}";
            return publicPath.Replace("\\", "/");
        }

        public string GetPublicUrl(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return filePath;

            // Nếu đã là dạng /uploads/... thì trả lại luôn
            // Nếu lỡ truyền "~/" thì bỏ "~"
            if (filePath.StartsWith("~"))
                return filePath.Substring(1);

            return filePath;
        }

        public Task DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return Task.CompletedTask;

            // filePath ở dạng /uploads/assignments/1/xxx.pdf
            var relativePath = filePath.TrimStart('~', '/'); // bỏ ~ và / phía trước
            var physicalPath = Path.Combine(_webRootPath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }

            return Task.CompletedTask;
        }

        // ====== private helper ======

        private string BuildUniqueFileName(string originalFileName)
        {
            var ext = Path.GetExtension(originalFileName);
            var name = Path.GetFileNameWithoutExtension(originalFileName);

            var timeStamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            var guid = Guid.NewGuid().ToString("N").Substring(0, 8);

            return $"{name}_{timeStamp}_{guid}{ext}";
        }
    }
}
