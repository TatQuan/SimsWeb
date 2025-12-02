using SimsWeb.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Student
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; }
    public Users User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;

    [NotMapped]
    public string StudentCode => $"STU{Id.ToString().PadLeft(4, '0')}";

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
