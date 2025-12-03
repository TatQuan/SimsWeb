using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimsWeb.Data;
using SimsWeb.Services.Interfaces;
using SimsWeb.ViewModels.Assignments;
using System.Security.Claims;

[Authorize(Roles = "Faculty")]
public class FacultyAssignmentController : Controller
{
    private readonly ITeacherAssignmentService _service;
    private readonly AppDbContext _db;

    public FacultyAssignmentController(ITeacherAssignmentService service, AppDbContext db)
    {
        _service = service;
        _db = db;
    }

    // Danh sách lớp
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var faculty = await _db.Faculties.FirstOrDefaultAsync(f => f.UserId == userId && !f.IsDeleted);
        if (faculty == null) return Forbid();

        var model = await _service.GetTeacherClassesAsync(faculty.Id);
        ViewData["Title"] = "My Classes";
        return View(model);
    }

    // Trang quản lý assignment theo lớp (dù chưa có assignment vẫn vào bình thường)
    public async Task<IActionResult> Manage(int classSectionId, int? assignmentId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var faculty = await _db.Faculties.FirstOrDefaultAsync(f => f.UserId == userId && !f.IsDeleted);
        if (faculty == null) return Forbid();

        var vm = await _service.GetClassAssignmentPageAsync(faculty.Id, classSectionId, assignmentId);
        ViewData["Title"] = "Manage Assignments";
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Save(AssignmentEditVM model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var faculty = await _db.Faculties.FirstOrDefaultAsync(f => f.UserId == userId && !f.IsDeleted);
        if (faculty == null) return Forbid();

        if (!ModelState.IsValid)
        {
            // load lại page để show lỗi
            var pageVm = await _service.GetClassAssignmentPageAsync(faculty.Id, model.ClassSectionId, model.Id);
            pageVm.EditModel = model;
            return View("Manage", pageVm);
        }

        var newId = await _service.SaveAssignmentAsync(faculty.Id, model);
        return RedirectToAction(nameof(Manage), new { classSectionId = model.ClassSectionId, assignmentId = newId });
    }

    [HttpPost]
    public async Task<IActionResult> SoftDelete(int id, int classSectionId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var faculty = await _db.Faculties.FirstOrDefaultAsync(f => f.UserId == userId && !f.IsDeleted);
        if (faculty == null) return Forbid();

        await _service.SoftDeleteAssignmentAsync(faculty.Id, id);
        return RedirectToAction(nameof(Manage), new { classSectionId });
    }

    [HttpPost]
    public async Task<IActionResult> Restore(int id, int classSectionId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var faculty = await _db.Faculties.FirstOrDefaultAsync(f => f.UserId == userId && !f.IsDeleted);
        if (faculty == null) return Forbid();

        await _service.RestoreAssignmentAsync(faculty.Id, id);
        return RedirectToAction(nameof(Manage), new { classSectionId });
    }

    [HttpPost]
    public async Task<IActionResult> HardDelete(int id, int classSectionId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var faculty = await _db.Faculties.FirstOrDefaultAsync(f => f.UserId == userId && !f.IsDeleted);
        if (faculty == null) return Forbid();

        await _service.HardDeleteAssignmentAsync(faculty.Id, id);
        return RedirectToAction(nameof(Manage), new { classSectionId });
    }
}
