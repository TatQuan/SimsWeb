using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using SimsWeb.Models;
using SimsWeb.Services.Implementations;
using SimsWeb.ViewModels;
using Xunit;

namespace SimsWeb.UnitTests
{
    public class AdminUserServiceTests
    {
        private readonly Mock<UserManager<Users>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly AdminUserService _service;

        public AdminUserServiceTests()
        {
            // Mock UserManager<Users>
            var userStore = new Mock<IUserStore<Users>>();
            _userManagerMock = new Mock<UserManager<Users>>(
                userStore.Object,
                null, null, null, null, null, null, null, null
            );

            // Mock RoleManager<IdentityRole>
            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                roleStore.Object,
                null, null, null, null
            );

            _service = new AdminUserService(_userManagerMock.Object, _roleManagerMock.Object);
        }

        // ========== CREATE USER ==========

        [Fact]
        public async Task CreateUserAsync_WithValidModelAndExistingRole_ShouldReturnSuccess()
        {
            // Arrange
            var model = new AdminCreateUserViewModel
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "Test@1234",
                SelectedRole = "User"
            };

            _userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<Users>(), model.Password))
                .ReturnsAsync(IdentityResult.Success);

            _roleManagerMock
                .Setup(m => m.RoleExistsAsync("User"))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(m => m.AddToRoleAsync(It.IsAny<Users>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var (success, errors) = await _service.CreateUserAsync(model);

            // Assert
            Assert.True(success);
            Assert.Empty(errors);
            _userManagerMock.Verify(m => m.CreateAsync(It.IsAny<Users>(), model.Password), Times.Once);
            _userManagerMock.Verify(m => m.AddToRoleAsync(It.IsAny<Users>(), "User"), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_WhenCreateAsyncFails_ShouldReturnErrors()
        {
            // Arrange
            var model = new AdminCreateUserViewModel
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "Test@1234",
                SelectedRole = "User"
            };

            var identityError = new IdentityError { Description = "Create failed" };

            _userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<Users>(), model.Password))
                .ReturnsAsync(IdentityResult.Failed(identityError));

            // Act
            var (success, errors) = await _service.CreateUserAsync(model);

            // Assert
            Assert.False(success);
            Assert.Contains("Create failed", errors);
            _userManagerMock.Verify(m => m.AddToRoleAsync(It.IsAny<Users>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CreateUserAsync_WhenRoleNotExists_ShouldCreateRoleAndAssign()
        {
            // Arrange
            var model = new AdminCreateUserViewModel
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "Test@1234",
                SelectedRole = "NewRole"
            };

            _userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<Users>(), model.Password))
                .ReturnsAsync(IdentityResult.Success);

            _roleManagerMock
                .Setup(m => m.RoleExistsAsync("NewRole"))
                .ReturnsAsync(false);

            _roleManagerMock
                .Setup(m => m.CreateAsync(It.Is<IdentityRole>(r => r.Name == "NewRole")))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(m => m.AddToRoleAsync(It.IsAny<Users>(), "NewRole"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var (success, errors) = await _service.CreateUserAsync(model);

            // Assert
            Assert.True(success);
            Assert.Empty(errors);
            _roleManagerMock.Verify(m => m.CreateAsync(It.Is<IdentityRole>(r => r.Name == "NewRole")), Times.Once);
            _userManagerMock.Verify(m => m.AddToRoleAsync(It.IsAny<Users>(), "NewRole"), Times.Once);
        }

        // ========== GET ACTIVE / DELETED USERS ==========

        [Fact]
        public async Task GetActiveUsersAsync_ShouldReturnOnlyNotDeletedUsers()
        {
            // Arrange
            var users = new List<Users>
            {
                new Users { Id = "1", FullName = "Active", Email = "a@test.com", UserName = "a@test.com", IsDeleted = false, CreatedAt = DateTime.UtcNow },
                new Users { Id = "2", FullName = "Deleted", Email = "d@test.com", UserName = "d@test.com", IsDeleted = true, CreatedAt = DateTime.UtcNow }
            };

            _userManagerMock
                .SetupGet(m => m.Users)
                .Returns(users.AsQueryable());

            _userManagerMock
                .Setup(m => m.GetRolesAsync(It.IsAny<Users>()))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _service.GetActiveUsersAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Active", result[0].FullName);
            Assert.False(result[0].Roles == null);
        }

        [Fact]
        public async Task GetDeletedUsersAsync_ShouldReturnOnlyDeletedUsers()
        {
            // Arrange
            var users = new List<Users>
            {
                new Users { Id = "1", FullName = "Active", Email = "a@test.com", UserName = "a@test.com", IsDeleted = false, CreatedAt = DateTime.UtcNow },
                new Users { Id = "2", FullName = "Deleted", Email = "d@test.com", UserName = "d@test.com", IsDeleted = true, CreatedAt = DateTime.UtcNow }
            };

            _userManagerMock
                .SetupGet(m => m.Users)
                .Returns(users.AsQueryable());

            _userManagerMock
                .Setup(m => m.GetRolesAsync(It.IsAny<Users>()))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _service.GetDeletedUsersAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Deleted", result[0].FullName);
        }

        // ========== GET CREATE VIEWMODEL ==========

        [Fact]
        public async Task GetCreateViewModelAsync_ShouldReturnAllRoles()
        {
            // Arrange
            var roles = new List<IdentityRole>
            {
                new IdentityRole("Admin"),
                new IdentityRole("User")
            }.AsQueryable();

            _roleManagerMock
                .SetupGet(m => m.Roles)
                .Returns(roles);

            // Act
            var vm = await _service.GetCreateViewModelAsync();

            // Assert
            Assert.Equal(2, vm.RoleList.Count);
            Assert.Contains("Admin", vm.RoleList);
            Assert.Contains("User", vm.RoleList);
        }

        // ========== GET EDIT VIEWMODEL ==========

        [Fact]
        public async Task GetEditViewModelAsync_WhenUserExists_ShouldReturnViewModel()
        {
            // Arrange
            var user = new Users
            {
                Id = "1",
                FullName = "Test User",
                Email = "test@example.com",
                UserName = "test@example.com"
            };

            _userManagerMock
                .Setup(m => m.FindByIdAsync("1"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            var roles = new List<IdentityRole>
            {
                new IdentityRole("Admin"),
                new IdentityRole("User")
            }.AsQueryable();

            _roleManagerMock
                .SetupGet(m => m.Roles)
                .Returns(roles);

            // Act
            var vm = await _service.GetEditViewModelAsync("1");

            // Assert
            Assert.NotNull(vm);
            Assert.Equal("1", vm.Id);
            Assert.Equal("Test User", vm.FullName);
            Assert.Equal("User", vm.SelectedRole);
            Assert.Contains("Admin", vm.RoleList);
            Assert.Contains("User", vm.RoleList);
        }

        [Fact]
        public async Task GetEditViewModelAsync_WhenUserNotFound_ShouldReturnNull()
        {
            // Arrange
            _userManagerMock
                .Setup(m => m.FindByIdAsync("1"))
                .ReturnsAsync((Users?)null);

            // Act
            var vm = await _service.GetEditViewModelAsync("1");

            // Assert
            Assert.Null(vm);
        }

        // ========== UPDATE USER ==========

        [Fact]
        public async Task UpdateUserAsync_WhenUserExists_ShouldUpdateAndReturnSuccess()
        {
            // Arrange
            var user = new Users
            {
                Id = "1",
                FullName = "Old Name",
                Email = "old@example.com",
                UserName = "old@example.com"
            };

            var model = new UserEditViewModel
            {
                Id = "1",
                FullName = "New Name",
                Email = "new@example.com",
                SelectedRole = "User"
            };

            _userManagerMock
                .Setup(m => m.FindByIdAsync("1"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(m => m.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "OldRole" });

            _userManagerMock
                .Setup(m => m.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            _roleManagerMock
                .Setup(m => m.RoleExistsAsync("User"))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(m => m.AddToRoleAsync(user, "User"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var (success, errors) = await _service.UpdateUserAsync(model);

            // Assert
            Assert.True(success);
            Assert.Empty(errors);
            Assert.Equal("New Name", user.FullName);
            Assert.Equal("new@example.com", user.Email);
        }

        [Fact]
        public async Task UpdateUserAsync_WhenUserNotFound_ShouldReturnFalse()
        {
            // Arrange
            var model = new UserEditViewModel
            {
                Id = "1",
                FullName = "New Name",
                Email = "new@example.com",
                SelectedRole = "User"
            };

            _userManagerMock
                .Setup(m => m.FindByIdAsync("1"))
                .ReturnsAsync((Users?)null);

            // Act
            var (success, errors) = await _service.UpdateUserAsync(model);

            // Assert
            Assert.False(success);
            Assert.Contains("User not found.", errors);
        }

        // ========== SOFT / RESTORE / HARD DELETE ==========

        [Fact]
        public async Task SoftDeleteAsync_WhenUserExists_ShouldSetIsDeletedTrue()
        {
            // Arrange
            var user = new Users
            {
                Id = "1",
                IsDeleted = false
            };

            _userManagerMock
                .Setup(m => m.FindByIdAsync("1"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(m => m.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.SoftDeleteAsync("1");

            // Assert
            Assert.True(result);
            Assert.True(user.IsDeleted);
        }

        [Fact]
        public async Task RestoreAsync_WhenUserExists_ShouldSetIsDeletedFalse()
        {
            // Arrange
            var user = new Users
            {
                Id = "1",
                IsDeleted = true
            };

            _userManagerMock
                .Setup(m => m.FindByIdAsync("1"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(m => m.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.RestoreAsync("1");

            // Assert
            Assert.True(result);
            Assert.False(user.IsDeleted);
        }

        [Fact]
        public async Task HardDeleteAsync_WhenUserExists_ShouldCallDeleteAndReturnTrue()
        {
            // Arrange
            var user = new Users { Id = "1" };

            _userManagerMock
                .Setup(m => m.FindByIdAsync("1"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(m => m.DeleteAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.HardDeleteAsync("1");

            // Assert
            Assert.True(result);
            _userManagerMock.Verify(m => m.DeleteAsync(user), Times.Once);
        }
    }
}
