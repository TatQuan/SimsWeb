using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using SimsWeb.Models;
using SimsWeb.ViewModels;

public class CoursesController : Controller
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    public async Task<IActionResult> Index()
    {
        var courses = await _courseService.GetAllAsync();
        return View(courses);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deleted()
    {
        var courses = await _courseService.GetDeletedAsync();
        return View(courses);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new CourseViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var entity = new Course
        {
            Code = model.Code,
            Name = model.Name,
            Description = model.Description,
            Credits = model.Credits,
            // CreatedAt sẽ set trong service
            IsDeleted = false
        };

        await _courseService.CreateAsync(entity);

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var course = await _courseService.GetByIdAsync(id);
        if (course == null) return NotFound();

        var vm = new CourseViewModel
        {
            Id = course.Id,
            Code = course.Code,
            Name = course.Name,
            Description = course.Description,
            Credits = course.Credits
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CourseViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var course = await _courseService.GetByIdAsync(model.Id);
        if (course == null) return NotFound();

        course.Code = model.Code;
        course.Name = model.Name;
        course.Description = model.Description;
        course.Credits = model.Credits;

        await _courseService.UpdateAsync(course);

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SoftDelete(int id)
    {
        await _courseService.SoftDeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        await _courseService.RestoreAsync(id);
        return RedirectToAction(nameof(Deleted));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HardDelete(int id)
    {
        await _courseService.HardDeleteAsync(id);
        return RedirectToAction(nameof(Deleted));
    }
}
