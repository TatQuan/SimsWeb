using Microsoft.EntityFrameworkCore;
using SimsWeb.Data;
using SimsWeb.Models;
using SimsWeb.Services.Interfaces;
using SimsWeb.ViewModels.Assignments;
using System.Net.NetworkInformation;

public class TeacherAssignmentService : ITeacherAssignmentService
{
    private readonly AppDbContext _db;
    private readonly IFileStorageService _storage;

    public TeacherAssignmentService(AppDbContext db, IFileStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<List<ClassSectionCardVM>> GetTeacherClassesAsync(int facultyId)
    {
        return await _db.ClassSections
            .Where(cs => cs.TeacherId == facultyId && cs.IsDeleted == false)
            .Select(cs => new ClassSectionCardVM
            {
                ClassSectionId = cs.Id,
                ClassName = cs.Name ?? cs.Code,
                CourseName = cs.Course.Name,
                StudentCount = cs.Enrollments.Count()
            })
            .ToListAsync();
    }

    public async Task<ClassAssignmentPageVM> GetClassAssignmentPageAsync(int facultyId, int classSectionId, int? assignmentId)
    {
        // verify class thuộc faculty này
        var cls = await _db.ClassSections
            .Include(cs => cs.Course)
            .FirstOrDefaultAsync(cs => cs.Id == classSectionId && !cs.IsDeleted);

        if (cls == null || cls.TeacherId != facultyId)
            throw new KeyNotFoundException("ClassSection not found or not your class");

        var assignments = await _db.Assignments
            .Where(a => a.ClassSectionId == classSectionId)
            .OrderByDescending(a => a.DueAt)
            .ToListAsync();

        var vm = new ClassAssignmentPageVM
        {
            ClassSectionId = cls.Id,
            ClassName = cls.Name ?? cls.Code,
            CourseName = cls.Course.Name,
            ActiveAssignments = assignments
                .Where(a => !a.IsDeleted)
                .Select(a => new AssignmentListItemVM
                {
                    Id = a.Id,
                    Title = a.Title,
                    DueAt = a.DueAt,
                    IsDeleted = false
                }).ToList(),
            DeletedAssignments = assignments
                .Where(a => a.IsDeleted)
                .Select(a => new AssignmentListItemVM
                {
                    Id = a.Id,
                    Title = a.Title,
                    DueAt = a.DueAt,
                    IsDeleted = true
                }).ToList()
        };

        // Nếu assignmentId có giá trị và tồn tại -> load vào EditModel để sửa
        Assignment? editing = null;
        if (assignmentId.HasValue)
        {
            editing = assignments.FirstOrDefault(a => a.Id == assignmentId.Value);
        }

        if (editing != null)
        {
            vm.EditModel = new AssignmentEditVM
            {
                Id = editing.Id,
                ClassSectionId = editing.ClassSectionId,
                Title = editing.Title,
                Description = editing.Description,
                DueAt = editing.DueAt,
                MaxScore = editing.MaxScore
            };
        }
        else
        {
            // form create trống (đây chính là case class mới chưa có assignment)
            vm.EditModel = new AssignmentEditVM
            {
                Id = null,
                ClassSectionId = classSectionId,
                DueAt = DateTime.UtcNow.AddDays(7) // default
            };
        }

        return vm;
    }

    public async Task<int> SaveAssignmentAsync(int facultyId, AssignmentEditVM model)
    {
        // kiểm tra class thuộc faculty
        var cls = await _db.ClassSections
            .FirstOrDefaultAsync(cs => cs.Id == model.ClassSectionId && !cs.IsDeleted);

        if (cls == null || cls.TeacherId != facultyId)
            throw new KeyNotFoundException("ClassSection not found or not your class");

        Assignment entity;
        if (model.Id.HasValue)
        {
            entity = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == model.Id.Value);
            if (entity == null)
                throw new KeyNotFoundException("Assignment not found");

            entity.Title = model.Title;
            entity.Description = model.Description;
            entity.DueAt = model.DueAt;
            entity.MaxScore = model.MaxScore;
        }
        else
        {
            entity = new Assignment
            {
                ClassSectionId = model.ClassSectionId,
                Title = model.Title,
                Description = model.Description,
                DueAt = model.DueAt,
                MaxScore = model.MaxScore,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            _db.Assignments.Add(entity);
        }

        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task SoftDeleteAssignmentAsync(int facultyId, int assignmentId)
    {
        var asm = await _db.Assignments
            .Include(a => a.ClassSection)
            .FirstOrDefaultAsync(a => a.Id == assignmentId);

        if (asm == null || asm.ClassSection.TeacherId != facultyId)
            throw new KeyNotFoundException("Assignment not found");

        asm.IsDeleted = true;
        await _db.SaveChangesAsync();
    }

    public async Task RestoreAssignmentAsync(int facultyId, int assignmentId)
    {
        var asm = await _db.Assignments
            .Include(a => a.ClassSection)
            .FirstOrDefaultAsync(a => a.Id == assignmentId);

        if (asm == null || asm.ClassSection.TeacherId != facultyId)
            throw new KeyNotFoundException("Assignment not found");

        asm.IsDeleted = false;
        await _db.SaveChangesAsync();
    }

    public async Task HardDeleteAssignmentAsync(int facultyId, int assignmentId)
    {
        var asm = await _db.Assignments
            .Include(a => a.ClassSection)
            .FirstOrDefaultAsync(a => a.Id == assignmentId);

        if (asm == null || asm.ClassSection.TeacherId != facultyId)
            throw new KeyNotFoundException("Assignment not found");

        _db.Assignments.Remove(asm);
        await _db.SaveChangesAsync();
    }

}
