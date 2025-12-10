using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SimsWeb.Controllers;
using SimsWeb.Data;
using SimsWeb.Models;
using SimsWeb.ViewModels;
using System.Threading.Tasks;
using Xunit;
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace SimsWeb.TestFunctionSims
{
    public class test_Unit
    {
        private AppDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private Mock<UserManager<Users>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<Users>>();
            return new Mock<UserManager<Users>>(
                store.Object, null, null, null, null, null, null, null, null
            );
        }

        private Mock<RoleManager<IdentityRole>> CreateRoleManagerMock()
        {
            var store = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(
                store.Object, null, null, null, null
            );
        }

        private Mock<SignInManager<Users>> CreateSignInManagerMock(Mock<UserManager<Users>> userManagerMock)
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<Users>>();

            return new Mock<SignInManager<Users>>(
                userManagerMock.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                null, null, null, null
            );
        }

        // ================== TEST 1: Login thành công ==================

        [Fact]
        public async Task Login_Post_WithValidModel_AndSucceededSignIn_RedirectsToHomeIndex()
        {
            // Arrange
            var userManagerMock = CreateUserManagerMock();
            var roleManagerMock = CreateRoleManagerMock();
            var signInManagerMock = CreateSignInManagerMock(userManagerMock);

            var email = "test@example.com";
            var password = "Password123!";
            var rememberMe = true;

            signInManagerMock
                .Setup(s => s.PasswordSignInAsync(email, password, rememberMe, false))
                .ReturnsAsync(IdentitySignInResult.Success);

            var controller = new AccountController(
                signInManagerMock.Object,
                userManagerMock.Object,
                roleManagerMock.Object
            );

            var model = new LoginViewModel
            {
                Email = email,
                Password = password,
                RememberMe = rememberMe
            };

            // Act
            var result = await controller.Login(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);

            signInManagerMock.Verify(s =>
                s.PasswordSignInAsync(email, password, rememberMe, false),
                Times.Once);
        }

        // ================== TEST 2: Login sai mật khẩu ==================

        [Fact]
        public async Task Login_Post_WithInvalidCredentials_ReturnsViewWithError()
        {
            // Arrange
            var userManagerMock = CreateUserManagerMock();
            var roleManagerMock = CreateRoleManagerMock();
            var signInManagerMock = CreateSignInManagerMock(userManagerMock);

            signInManagerMock
                .Setup(s => s.PasswordSignInAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    false))
                .ReturnsAsync(IdentitySignInResult.Failed);

            var controller = new AccountController(
                signInManagerMock.Object,
                userManagerMock.Object,
                roleManagerMock.Object
            );

            var model = new LoginViewModel
            {
                Email = "wrong@example.com",
                Password = "WrongPassword",
                RememberMe = false
            };

            // Act
            var result = await controller.Login(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.Model);

            // Check ModelState has error "Invalid Login Attempt."
            Assert.True(controller.ModelState.ContainsKey(string.Empty));
            var errors = controller.ModelState[string.Empty].Errors;
            Assert.Contains(errors, e => e.ErrorMessage == "Invalid Login Attempt.");

            signInManagerMock.Verify(s =>
                s.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    false),
                Times.Once);
        }

        // ====================== TEST CREATE ======================

        [Fact]
        public async Task CreateAsync_ShouldAddCourseToDatabase()
        {
            // Arrange
            var context = CreateInMemoryDbContext();
            var service = new CourseService(context);

            var course = new Course
            {
                Code = "CS101",
                Name = "Intro to Programming",
                Credits = 3,
                Description = "Basic programming"
            };

            // Act
            await service.CreateAsync(course);

            // Assert
            var stored = await context.Courses.FirstOrDefaultAsync();
            Assert.NotNull(stored);
            Assert.Equal("CS101", stored.Code);
            Assert.NotEqual(default, stored.CreatedAt);
        }

        // ====================== TEST UPDATE ======================

        [Fact]
        public async Task UpdateAsync_ShouldModifyCourse()
        {
            var context = CreateInMemoryDbContext();
            var service = new CourseService(context);

            var course = new Course
            {
                Code = "CS101",
                Name = "Intro",
                Credits = 3
            };

            context.Courses.Add(course);
            await context.SaveChangesAsync();

            // Act
            course.Name = "Advanced Programming";
            await service.UpdateAsync(course);

            // Assert
            var updated = await context.Courses.FirstAsync();
            Assert.Equal("Advanced Programming", updated.Name);
        }

        // ====================== TEST SOFT DELETE ======================

        [Fact]
        public async Task SoftDeleteAsync_ShouldMarkCourseAsDeleted()
        {
            var context = CreateInMemoryDbContext();
            var service = new CourseService(context);

            var course = new Course { Code = "CS101", Name = "Test", Credits = 3 };
            context.Courses.Add(course);
            await context.SaveChangesAsync();

            // Act
            await service.SoftDeleteAsync(course.Id);

            // Assert
            var stored = await context.Courses.FirstAsync();
            Assert.True(stored.IsDeleted);
        }

        // ====================== TEST GET ALL (only active) ======================

        [Fact]
        public async Task GetAllAsync_ShouldReturnOnlyActiveCourses()
        {
            var context = CreateInMemoryDbContext();
            var service = new CourseService(context);

            context.Courses.AddRange(
                new Course { Code = "A", Name = "Active", Credits = 3, IsDeleted = false },
                new Course { Code = "D", Name = "Deleted", Credits = 3, IsDeleted = true }
            );

            await context.SaveChangesAsync();

            // Act
            var activeList = await service.GetAllAsync();

            // Assert
            Assert.Single(activeList);
            Assert.Equal("A", activeList[0].Code);
        }
    }
}
