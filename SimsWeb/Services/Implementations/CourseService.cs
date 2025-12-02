using SimsWeb.Data;
using SimsWeb.Models;
using Microsoft.EntityFrameworkCore;
using SimsWeb.Services.Interfaces;

public class CourseService : ICourseService
{
    private readonly AppDbContext _context;

    public CourseService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Course>> GetAllAsync()
        => await _context.Courses.Where(c => !c.IsDeleted).ToListAsync();

    public async Task<Course?> GetByIdAsync(int id)
        => await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);

    public async Task CreateAsync(Course course)
    {
        course.CreatedAt = DateTime.UtcNow;
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Course course)
    {
        _context.Courses.Update(course);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int id)
    {
        var course = await GetByIdAsync(id);
        if (course == null) return;

        course.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    public async Task<List<Course>> GetDeletedAsync()
        => await _context.Courses.Where(c => c.IsDeleted).ToListAsync();

    public async Task RestoreAsync(int id)
    {
        var course = await GetByIdAsync(id);
        if (course == null) return;

        course.IsDeleted = false;
        await _context.SaveChangesAsync();
    }

    public async Task HardDeleteAsync(int id)
    {
        var course = await GetByIdAsync(id);
        if (course == null) return;

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
    }
}
