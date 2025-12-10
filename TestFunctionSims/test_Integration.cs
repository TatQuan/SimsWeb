using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using SimsWeb.Controllers;
using SimsWeb.Data;
using SimsWeb.Models;
using SimsWeb.Services.Implementations;
using SimsWeb.Services.Interfaces;
using SimsWeb.ViewModels;
using Xunit;

namespace SimsWeb.TestFunctionSims
{
    public class test_Integration
    {
        // Helpers ===================================================

        private AppDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private EnrollmentsController CreateController(AppDbContext context)
        {
            IEnrollmentService enrollmentService = new EnrollmentService(context);

            var courseServiceMock = new Mock<ICourseService>();

            var controller = new EnrollmentsController(enrollmentService, courseServiceMock.Object);

            // Set TempData to controller set message
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            controller.TempData = tempData;

            return controller;
        }

        private async Task<(ClassSection section, Student s1, Student s2, Student s3)> SeedClassWithStudents(AppDbContext context)
        {
            var section = new ClassSection
            {
                Id = 1,
                Code = "CLS001",
                Name = "Class 001",
                IsDeleted = false
            };

            var u1 = new Users
            {
                UserName = "stu1@example.com",
                Email = "stu1@example.com",
                FullName = "Student One",
                NormalizedUserName = "STU1@EXAMPLE.COM",
                NormalizedEmail = "STU1@EXAMPLE.COM",
                EmailConfirmed = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            var u2 = new Users
            {
                UserName = "stu2@example.com",
                Email = "stu2@example.com",
                FullName = "Student Two",
                NormalizedUserName = "STU2@EXAMPLE.COM",
                NormalizedEmail = "STU2@EXAMPLE.COM",
                EmailConfirmed = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            var u3 = new Users
            {
                UserName = "stu3@example.com",
                Email = "stu3@example.com",
                FullName = "Student Three",
                NormalizedUserName = "STU3@EXAMPLE.COM",
                NormalizedEmail = "STU3@EXAMPLE.COM",
                EmailConfirmed = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.AddRange(u1, u2, u3);

            var s1 = new Student
            {
                Id = 1,
                UserId = u1.Id,
                IsDeleted = false
            };

            var s2 = new Student
            {
                Id = 2,
                UserId = u2.Id,
                IsDeleted = false
            };

            var s3 = new Student
            {
                Id = 3,
                UserId = u3.Id,
                IsDeleted = false
            };

            context.ClassSections.Add(section);
            context.Students.AddRange(s1, s2, s3);

            await context.SaveChangesAsync();

            return (section, s1, s2, s3);
        }

        // ================= IT_EN01: Manage build view model =================

        //[Fact]
        //public async Task Manage_ShouldBuildViewModelWithEnrolledAndAvailableStudents()
        //{
        //    // Arrange
        //    var context = CreateInMemoryDbContext();
        //    var (section, s1, s2, s3) = await SeedClassWithStudents(context);

        //    // Enroll s1 và s2, s3 free
        //    context.Enrollments.AddRange(
        //        new Enrollment
        //        {
        //            ClassSectionId = section.Id,
        //            StudentId = s1.Id,
        //            IsDeleted = false,
        //            EnrolledAt = DateTime.UtcNow
        //        },
        //        new Enrollment
        //        {
        //            ClassSectionId = section.Id,
        //            StudentId = s2.Id,
        //            IsDeleted = false,
        //            EnrolledAt = DateTime.UtcNow
        //        }
        //    );

        //    await context.SaveChangesAsync();

        //    var controller = CreateController(context);

        //    // Act
        //    var result = await controller.Manage(section.Id);

        //    // Assert
        //    var viewResult = Assert.IsType<ViewResult>(result);
        //    var model = Assert.IsType<ClassEnrollmentViewModel>(viewResult.Model);

        //    Assert.Equal(section.Id, model.ClassSectionId);
        //    Assert.Equal(2, model.EnrolledStudents.Count);   // s1, s2
        //    Assert.Single(model.AvailableStudents);          // s3
        //    Assert.Contains(model.AvailableStudents, s => s.StudentId == s3.Id);
        //}

        // ================= IT_EN02: AddStudents =================

        [Fact]
        public async Task AddStudents_ShouldCreateNewEnrollments_AndSetMessage()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            var (section, s1, s2, s3) = await SeedClassWithStudents(context);

            var controller = CreateController(context);

            var model = new ClassEnrollmentViewModel
            {
                ClassSectionId = section.Id,
                SelectedStudentIds = new[] { s1.Id, s2.Id }
            };

            // Act
            var result = await controller.AddStudents(model);

            // Assert: redirect
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Manage", redirect.ActionName);
            Assert.Equal(section.Id, redirect.RouteValues["id"]);

            // Assert: DB has enrollments
            var enrollments = await context.Enrollments
                .Where(e => e.ClassSectionId == section.Id && !e.IsDeleted)
                .ToListAsync();

            Assert.Equal(2, enrollments.Count);
            Assert.Contains(enrollments, e => e.StudentId == s1.Id);
            Assert.Contains(enrollments, e => e.StudentId == s2.Id);
            Assert.All(enrollments, e => Assert.NotEqual(default, e.EnrolledAt));

            // Assert: TempData has message
            Assert.True(controller.TempData.ContainsKey("EnrollmentMessage"));
            var msg = controller.TempData["EnrollmentMessage"]?.ToString();
            Assert.Contains("Added", msg);
        }

        

        // ============= IT_EN05: RemoveStudent - soft delete enrollment =============

        [Fact]
        public async Task RemoveStudent_ShouldSoftDeleteEnrollment_AndSetMessage()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            var (section, s1, s2, s3) = await SeedClassWithStudents(context);

            var enrollment = new Enrollment
            {
                ClassSectionId = section.Id,
                StudentId = s1.Id,
                EnrolledAt = DateTime.UtcNow,
                IsDeleted = false
            };

            context.Enrollments.Add(enrollment);
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            // Act
            var result = await controller.RemoveStudent(section.Id, s1.Id);

            // Assert: redirect
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Manage", redirect.ActionName);
            Assert.Equal(section.Id, redirect.RouteValues["id"]);

            // Assert: IsDeleted = true
            var stored = await context.Enrollments
                .FirstAsync(e => e.ClassSectionId == section.Id && e.StudentId == s1.Id);

            Assert.True(stored.IsDeleted);

            // Message
            var msg = controller.TempData["EnrollmentMessage"]?.ToString();
            Assert.Equal("Student removed from class.", msg);
        }

        // ============= IT_EN06: RemoveStudent - enrollment không tồn tại =============

        //[Fact]
        //public async Task RemoveStudent_WhenEnrollmentNotFound_ShouldNotChangeDatabase_AndSetErrorMessage()
        //{
        //    // Arrange
        //    var context = CreateInMemoryDbContext();
        //    var (section, s1, s2, s3) = await SeedClassWithStudents(context);

        //    // Not create enrollment
        //    var controller = CreateController(context);

        //    // Act
        //    var result = await controller.RemoveStudent(section.Id, s1.Id);

        //    // Assert: redirect
        //    var redirect = Assert.IsType<RedirectToActionResult>(result);
        //    Assert.Equal("Manage", redirect.ActionName);
        //    Assert.Equal(section.Id, redirect.RouteValues["id"]);

        //    // DB vẫn không có enrollment
        //    var enrollments = await context.Enrollments
        //        .Where(e => e.ClassSectionId == section.Id)
        //        .ToListAsync();

        //    Assert.Empty(enrollments);

        //    // Message
        //    var msg = controller.TempData["EnrollmentMessage"]?.ToString();
        //    Assert.Equal("Enrollment not found or already removed.", msg);
        //}
    }
}
