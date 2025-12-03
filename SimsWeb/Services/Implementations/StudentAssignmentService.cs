using SimsWeb.Data;
using SimsWeb.Models;
using SimsWeb.Services.Interfaces;
using SimsWeb.ViewModels.Assignments;
using Microsoft.EntityFrameworkCore;

public class StudentAssignmentService : IStudentAssignmentService
{
    private readonly AppDbContext _db;
    private readonly IFileStorageService _storage;

    public StudentAssignmentService(AppDbContext db, IFileStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<List<ClassSectionCardVM>> GetStudentClassesAsync(int studentId)
    {
        return await _db.Enrollments
            .Where(e => e.StudentId == studentId)
            .Select(e => new ClassSectionCardVM
            {
                ClassSectionId = e.ClassSection.Id,
                ClassName = e.ClassSection.Name ?? e.ClassSection.Code,
                CourseName = e.ClassSection.Course.Name,
                StudentCount = e.ClassSection.Enrollments.Count()
            })
            .ToListAsync();
    }

    public async Task<AssignmentDetailVM> GetAssignmentDetailAsync(int assignmentId, int studentId)
    {
        var vm = await _db.Assignments
            .AsNoTracking()
            .Where(a => a.Id == assignmentId && !a.IsDeleted)
            .Select(a => new
            {
                Assignment = a,
                MySubmission = a.Submissions
                    .Where(s => !s.IsDeleted && s.StudentId == studentId)
                    .Select(s => new AssignmentSubmissionVM
                    {
                        Id = s.Id,
                        StudentName = "", // student không cần
                        FileName = Path.GetFileName(s.FilePath),
                        DownloadUrl = s.FilePath,
                        SubmittedAt = s.SubmittedAt,
                        Score = s.Score,
                        TeacherComment = s.TeacherComment
                    })
                    .SingleOrDefault()
            })
            .Select(x => new AssignmentDetailVM
            {
                AssignmentId = x.Assignment.Id,
                ClassSectionId = x.Assignment.ClassSectionId,
                ClassName = x.Assignment.ClassSection.Name ?? x.Assignment.ClassSection.Code,
                CourseName = x.Assignment.ClassSection.Course.Name,
                Title = x.Assignment.Title,
                Description = x.Assignment.Description,
                DueAt = x.Assignment.DueAt,
                MaxScore = x.Assignment.MaxScore,

                ExerciseDownloadUrl = x.Assignment.ExerciseFilePath,
                GuideDownloadUrl = x.Assignment.GuideFilePath,

                Documents = x.Assignment.Resources
                    .Select(r => new AssignmentResourceVM
                    {
                        Id = r.Id,
                        FileName = r.FileName,
                        DownloadUrl = r.FilePath,
                        UploadedAt = r.UploadedAt
                    })
                    .ToList(),

                MySubmission = x.MySubmission,
                AllSubmissions = new List<AssignmentSubmissionVM>(),
                IsTeacherView = false
            })
            .SingleOrDefaultAsync();

        if (vm == null)
            throw new KeyNotFoundException("Assignment not found");

        return vm;
    }


    public async Task SubmitAsync(SubmitAssignmentVM model, int studentId)
    {
        var asm = await _db.Assignments
            .Include(a => a.ClassSection)
            .ThenInclude(cs => cs.Enrollments)
            .FirstOrDefaultAsync(a => a.Id == model.AssignmentId);

        if (asm == null)
            throw new KeyNotFoundException();

        // Kiểm tra sinh viên thuộc lớp
        if (!asm.ClassSection.Enrollments.Any(e => e.StudentId == studentId))
            throw new UnauthorizedAccessException("Not in this class");

        var filePath = await _storage.SaveSubmissionFileAsync(asm.Id, studentId, model.File);

        var existing = await _db.AssignmentSubmissions
            .FirstOrDefaultAsync(s => s.AssignmentId == asm.Id && s.StudentId == studentId);

        if (existing == null)
        {
            _db.AssignmentSubmissions.Add(new AssignmentSubmission
            {
                AssignmentId = asm.Id,
                StudentId = studentId,
                FilePath = filePath,
                SubmittedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.FilePath = filePath;
            existing.SubmittedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }
}
