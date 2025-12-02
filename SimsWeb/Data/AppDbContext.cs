
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimsWeb.Models;

namespace SimsWeb.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public DbSet<Course> Courses { get; set; }
        public DbSet<ClassSection> ClassSections { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }


        public AppDbContext(DbContextOptions options) : base(options)
        {

        }

        protected AppDbContext()
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasOne(e => e.Student)
                      .WithMany(s => s.Enrollments)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ClassSection)
                      .WithMany(c => c.Enrollments)
                      .HasForeignKey(e => e.ClassSectionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

    }
}
