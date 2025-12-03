using SimsWeb.Services.Interfaces;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;

    public FileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveAssignmentFileAsync(int assignmentId, int studentId, IFormFile file)
    {
        var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "assignments");
        Directory.CreateDirectory(uploadsRoot);

        var fileName = $"{assignmentId}_{studentId}_{DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(file.FileName)}";
        var physicalPath = Path.Combine(uploadsRoot, fileName);
        var relativePath = $"/uploads/assignments/{fileName}";

        using (var stream = new FileStream(physicalPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return relativePath;
    }

    // Lưu file bài / hướng dẫn cho giảng viên
    public async Task<string> SaveAssignmentMaterialAsync(int assignmentId, IFormFile file, string kind)
    {
        var root = Path.Combine(_env.WebRootPath, "uploads", "assignments", "materials");
        Directory.CreateDirectory(root);

        // kind = "exercise" hoặc "guide"
        var safeKind = string.IsNullOrWhiteSpace(kind) ? "file" : kind;
        var fileName = $"{assignmentId}_{safeKind}_{DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(file.FileName)}";

        var physicalPath = Path.Combine(root, fileName);
        var relativePath = $"/uploads/assignments/materials/{fileName}";

        using (var stream = new FileStream(physicalPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return relativePath;
    }
}
