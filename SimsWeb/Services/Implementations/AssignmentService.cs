using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SimsWeb.Data;
using SimsWeb.Models;
using SimsWeb.Services.Interfaces;
using SimsWeb.ViewModels;

namespace SimsWeb.Services.Implementations
{
    public class AssignmentService : ITeacherAssignmentService, IStudentAssignmentService
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _fileStorage;

        public AssignmentService(AppDbContext context, IFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }
        // ========== FACULTY ==========

        public async Task<List<AssignmentListItemViewModel>> GetAssignmentsForTeacherAsync(string teacherUserId)
        {
            var faculty = await _context.Faculties
                .FirstOrDefaultAsync(f => f.UserId == teacherUserId && !f.IsDeleted);

            if (faculty == null) return new List<AssignmentListItemViewModel>();

            return await _context.Assignments
                .Where(a => !a.IsDeleted && a.ClassSection.TeacherId == faculty.Id && !a.ClassSection.IsDeleted)
                .Include(a => a.ClassSection).ThenInclude(cs => cs.Course)
                .OrderBy(a => a.DueAt)
                .Select(a => new AssignmentListItemViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    ClassCode = a.ClassSection.Code,
                    ClassName = a.ClassSection.Name,
                    CourseName = a.ClassSection.Course.Name,
                    DueAt = a.DueAt
                })
                .ToListAsync();
        }

        public async Task<AssignmentCreateViewModel> BuildCreateViewModelAsync(string teacherUserId)
        {
            var faculty = await _context.Faculties
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.UserId == teacherUserId && !f.IsDeleted);

            if (faculty == null)
            {
                return new AssignmentCreateViewModel();
            }

            var sections = await _context.ClassSections
                .Where(cs => cs.TeacherId == faculty.Id && !cs.IsDeleted)
                .OrderBy(cs => cs.Code)
                .ToListAsync();

            return new AssignmentCreateViewModel
            {
                TeacherClassSections = sections,
                DueDate = DateTime.Today.AddDays(7),
                DueTime = new TimeSpan(23, 59, 0)
            };
        }

        public async Task<AssignmentEditViewModel?> BuildEditViewModelAsync(int id, string teacherUserId)
        {
            var faculty = await _context.Faculties
                .FirstOrDefaultAsync(f => f.UserId == teacherUserId && !f.IsDeleted);
            if (faculty == null) return null;

            var assignment = await _context.Assignments
                .Include(a => a.ClassSection)
                .FirstOrDefaultAsync(a =>
                    a.Id == id &&
                    !a.IsDeleted &&
                    a.ClassSection.TeacherId == faculty.Id &&
                    !a.ClassSection.IsDeleted);

            if (assignment == null) return null;

            var sections = await _context.ClassSections
                .Where(cs => cs.TeacherId == faculty.Id && !cs.IsDeleted)
                .OrderBy(cs => cs.Code)
                .ToListAsync();

            return new AssignmentEditViewModel
            {
                Id = assignment.Id,
                ClassSectionId = assignment.ClassSectionId,
                Title = assignment.Title,
                Description = assignment.Description,
                DueDate = assignment.DueAt.Date,
                DueTime = assignment.DueAt.TimeOfDay,
                MaxScore = assignment.MaxScore,
                TeacherClassSections = sections,
                ExistingExerciseFilePath = assignment.ExerciseFilePath,
                ExistingGuideFilePath = assignment.GuideFilePath
            };
        }


        public async Task<int?> CreateAsync(AssignmentCreateViewModel model, string teacherUserId)
        {
            var faculty = await _context.Faculties
                .FirstOrDefaultAsync(f => f.UserId == teacherUserId && !f.IsDeleted);
            if (faculty == null) return null;

            var section = await _context.ClassSections
                .FirstOrDefaultAsync(cs => cs.Id == model.ClassSectionId
                                           && cs.TeacherId == faculty.Id
                                           && !cs.IsDeleted);
            if (section == null) return null;

            var dueAt = model.DueDate.Date + model.DueTime;

            var asm = new Assignment
            {
                Title = model.Title,
                Description = model.Description,
                ClassSectionId = section.Id,
                DueAt = dueAt,
                MaxScore = model.MaxScore
            };

            _context.Assignments.Add(asm);
            await _context.SaveChangesAsync(); // asm.Id có giá trị

            if (model.ExerciseFile != null && model.ExerciseFile.Length > 0)
            {
                var exercisePath = await _fileStorage.SaveAssignmentMaterialAsync(asm.Id, model.ExerciseFile, "exercise");
                asm.ExerciseFilePath = exercisePath;
            }

            if (model.GuideFile != null && model.GuideFile.Length > 0)
            {
                var guidePath = await _fileStorage.SaveAssignmentMaterialAsync(asm.Id, model.GuideFile, "guide");
                asm.GuideFilePath = guidePath;
            }

            await _context.SaveChangesAsync();
            return asm.Id;
        }

        public async Task<bool> UpdateAsync(AssignmentEditViewModel model, string teacherUserId)
        {
            var faculty = await _context.Faculties
                .FirstOrDefaultAsync(f => f.UserId == teacherUserId && !f.IsDeleted);
            if (faculty == null) return false;

            var assignment = await _context.Assignments
                .Include(a => a.ClassSection)
                .FirstOrDefaultAsync(a =>
                    a.Id == model.Id &&
                    !a.IsDeleted &&
                    a.ClassSection.TeacherId == faculty.Id &&
                    !a.ClassSection.IsDeleted);

            if (assignment == null) return false;

            // cập nhật basic fields
            assignment.Title = model.Title;
            assignment.Description = model.Description;
            assignment.ClassSectionId = model.ClassSectionId;
            assignment.DueAt = model.DueDate.Date + model.DueTime;
            assignment.MaxScore = model.MaxScore;

            // nếu upload file mới thì thay path
            if (model.ExerciseFile != null && model.ExerciseFile.Length > 0)
            {
                var exercisePath = await _fileStorage
                    .SaveAssignmentMaterialAsync(assignment.Id, model.ExerciseFile, "exercise");
                assignment.ExerciseFilePath = exercisePath;
            }

            if (model.GuideFile != null && model.GuideFile.Length > 0)
            {
                var guidePath = await _fileStorage
                    .SaveAssignmentMaterialAsync(assignment.Id, model.GuideFile, "guide");
                assignment.GuideFilePath = guidePath;
            }

            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<AssignmentDetailViewModel?> GetAssignmentDetailForTeacherAsync(int assignmentId, string teacherUserId)
        {
            var faculty = await _context.Faculties
                .FirstOrDefaultAsync(f => f.UserId == teacherUserId && !f.IsDeleted);

            if (faculty == null) return null;

            var assignment = await _context.Assignments
                .Include(a => a.ClassSection).ThenInclude(cs => cs.Course)
                .Include(a => a.ClassSection).ThenInclude(cs => cs.Teacher).ThenInclude(t => t.User)
                .Include(a => a.Submissions).ThenInclude(s => s.Student).ThenInclude(st => st.User)
                .FirstOrDefaultAsync(a => a.Id == assignmentId
                                          && !a.IsDeleted
                                          && a.ClassSection.TeacherId == faculty.Id);

            if (assignment == null) return null;

            var vm = new AssignmentDetailViewModel
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Description = assignment.Description,
                DueAt = assignment.DueAt,
                ClassCode = assignment.ClassSection.Code,
                ClassName = assignment.ClassSection.Name,
                CourseName = assignment.ClassSection.Course.Name,
                TeacherName = assignment.ClassSection.Teacher.User.FullName,

                ExerciseFilePath = assignment.ExerciseFilePath,
                GuideFilePath = assignment.GuideFilePath,

                Submissions = assignment.Submissions
                    .Where(s => !s.IsDeleted)
                    .Select(s => new AssignmentSubmissionViewModel
                    {
                        SubmissionId = s.Id,
                        AssignmentId = assignment.Id,
                        StudentId = s.StudentId,
                        StudentCode = s.Student.StudentCode,
                        StudentName = s.Student.User.FullName,
                        SubmittedAt = s.SubmittedAt,
                        FilePath = s.FilePath,
                        Score = s.Score,
                        TeacherComment = s.TeacherComment,
                        IsLate = s.SubmittedAt > assignment.DueAt
                    })
                    .OrderBy(s => s.StudentCode)
                    .ToList()
            };

            return vm;
        }

        public async Task<bool> GradeSubmissionAsync(int submissionId, int score, string? comment)
        {
            var submission = await _context.AssignmentSubmissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == submissionId && !s.IsDeleted);

            if (submission == null) return false;

            // (có thể check teacher có quyền không, tạm thời bỏ qua)
            submission.Score = score;
            submission.TeacherComment = comment;
            submission.GradedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ========== STUDENT ==========

        public async Task<List<AssignmentListItemViewModel>> GetAssignmentsForStudentAsync(string studentUserId)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == studentUserId && !s.IsDeleted);

            if (student == null) return new List<AssignmentListItemViewModel>();

            // tất cả class mà student đang enroll
            var classIds = await _context.Enrollments
                .Where(e => e.StudentId == student.Id && !e.IsDeleted && !e.ClassSection.IsDeleted)
                .Select(e => e.ClassSectionId)
                .Distinct()
                .ToListAsync();

            var now = DateTime.UtcNow;

            var assignments = await _context.Assignments
                .Where(a => !a.IsDeleted && classIds.Contains(a.ClassSectionId))
                .Include(a => a.ClassSection).ThenInclude(cs => cs.Course)
                .Include(a => a.Submissions.Where(s => !s.IsDeleted && s.StudentId == student.Id))
                .OrderBy(a => a.DueAt)
                .ToListAsync();

            return assignments.Select(a =>
            {
                var my = a.Submissions.FirstOrDefault();
                return new AssignmentListItemViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    ClassCode = a.ClassSection.Code,
                    ClassName = a.ClassSection.Name,
                    CourseName = a.ClassSection.Course.Name,
                    DueAt = a.DueAt,
                    IsSubmitted = my != null,
                    Score = my?.Score,
                    IsLate = my != null && my.SubmittedAt > a.DueAt
                };
            }).ToList();
        }

        public async Task<AssignmentDetailViewModel?> GetAssignmentDetailForStudentAsync(int assignmentId, string studentUserId)
        {
            var student = await _context.Students
                .Include(s => s.User) // ← thêm dòng này
                .FirstOrDefaultAsync(s => s.UserId == studentUserId && !s.IsDeleted);

            if (student == null) return null;

            var assignment = await _context.Assignments
                .Include(a => a.ClassSection).ThenInclude(cs => cs.Course)
                .Include(a => a.ClassSection).ThenInclude(cs => cs.Teacher).ThenInclude(t => t.User)
                .Include(a => a.Submissions.Where(s => !s.IsDeleted && s.StudentId == student.Id))
                .FirstOrDefaultAsync(a => a.Id == assignmentId && !a.IsDeleted);

            if (assignment == null) return null;

            var my = assignment.Submissions.FirstOrDefault();

            return new AssignmentDetailViewModel
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Description = assignment.Description,
                DueAt = assignment.DueAt,
                ClassCode = assignment.ClassSection.Code,
                ClassName = assignment.ClassSection.Name,
                CourseName = assignment.ClassSection.Course.Name,
                TeacherName = assignment.ClassSection.Teacher.User.FullName,

                // ====== FILE GIÁO VIÊN ======
                ExerciseFilePath = assignment.ExerciseFilePath,
                GuideFilePath = assignment.GuideFilePath,

                // ====== BÀI SUBMIT CỦA STUDENT ======
                MySubmission = my == null ? null : new AssignmentSubmissionViewModel
                {
                    SubmissionId = my.Id,
                    AssignmentId = assignment.Id,
                    StudentId = student.Id,
                    StudentCode = student.StudentCode,
                    StudentName = student.User?.FullName ?? "",
                    SubmittedAt = my.SubmittedAt,
                    FilePath = my.FilePath,
                    Score = my.Score,
                    TeacherComment = my.TeacherComment,
                    IsLate = my.SubmittedAt > assignment.DueAt
                }
            };
        }



        public async Task<bool> SubmitAsync(int assignmentId, string studentUserId, IFormFile file)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == studentUserId && !s.IsDeleted);

            if (student == null) return false;

            var assignment = await _context.Assignments
                .FirstOrDefaultAsync(a => a.Id == assignmentId && !a.IsDeleted);

            if (assignment == null) return false;

            if (file == null || file.Length == 0) return false;

            // dùng file storage service
            var relativePath = await _fileStorage.SaveAssignmentFileAsync(assignmentId, student.Id, file);

            var existing = await _context.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId
                                          && s.StudentId == student.Id
                                          && !s.IsDeleted);

            var now = DateTime.UtcNow;

            if (existing == null)
            {
                existing = new AssignmentSubmission
                {
                    AssignmentId = assignmentId,
                    StudentId = student.Id,
                    SubmittedAt = now,
                    FilePath = relativePath
                };
                _context.AssignmentSubmissions.Add(existing);
            }
            else
            {
                existing.SubmittedAt = now;
                existing.FilePath = relativePath;
                existing.Score = null;
                existing.TeacherComment = null;
                existing.GradedAt = null;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteAsync(int id, string teacherUserId)
        {
            var faculty = await _context.Faculties
                .FirstOrDefaultAsync(f => f.UserId == teacherUserId && !f.IsDeleted);
            if (faculty == null) return false;

            var assignment = await _context.Assignments
                .Include(a => a.ClassSection)
                .FirstOrDefaultAsync(a =>
                    a.Id == id &&
                    !a.IsDeleted &&
                    a.ClassSection.TeacherId == faculty.Id &&
                    !a.ClassSection.IsDeleted);

            if (assignment == null) return false;

            assignment.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
