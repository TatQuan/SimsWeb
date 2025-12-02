using SimsWeb.Models;

public interface ICourseService
{
    Task<List<Course>> GetAllAsync();
    Task<Course?> GetByIdAsync(int id);
    Task CreateAsync(Course course);
    Task UpdateAsync(Course course);
    Task SoftDeleteAsync(int id);
    Task<List<Course>> GetDeletedAsync();
    Task RestoreAsync(int id);
    Task HardDeleteAsync(int id);
}
