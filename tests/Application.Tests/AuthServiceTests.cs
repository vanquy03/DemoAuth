using Application.Dtos;
using Application.Services;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework; // Thêm NUnit
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Tests
{
    [TestFixture]
    public class AuthServiceTests
    {
        // 1. Dữ liệu giả định
        private readonly List<ApplicationUser> _userList = new List<ApplicationUser>
        {
            new ApplicationUser { UserName = "testuser", Email = "testuser@example.com", Id = "user-id-1" }
        };
        private readonly List<IdentityRole> _roleList = new List<IdentityRole>
        {
            new IdentityRole { Name = "EndUser" }
        };

        // 2. Các đối tượng Mock (được khởi tạo trong SetUp)
        private Mock<UserManager<ApplicationUser>> _mockUserManager;
        private Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private Mock<SignInManager<ApplicationUser>> _mockSignInManager;
        private Mock<IConfiguration> _mockConfiguration;

        // Phương thức Setup (chạy trước mỗi Test)
        [SetUp]
        public void Setup() // SỬA: Phải là public
        {
            // KHẮC PHỤC LỖI ACCESSIBILITY: Đảm bảo MockHelpers là public
            _mockUserManager = MockHelpers.MockUserManager(_userList);
            _mockRoleManager = MockHelpers.MockRoleManager(_roleList);
            _mockSignInManager = MockHelpers.MockSignInManager<ApplicationUser>();
            _mockConfiguration = MockHelpers.MockConfiguration();
        }

        // SỬA: Thêm public để khắc phục lỗi Inconsistent accessibility
        public AuthService CreateService()
        {
            return new AuthService(
                _mockUserManager.Object,
                _mockRoleManager.Object,
                _mockSignInManager.Object,
                _mockConfiguration.Object
            );
        }

        // -------------------------------------------------------------------
        // --- TEST CHO ĐĂNG KÝ (RegisterAsync) ---
        // -------------------------------------------------------------------

        [Test]
        public async Task RegisterAsync_ShouldReturnTrueAndVerifyCalls_WhenUserIsNew()
        {
            // Arrange
            var service = CreateService();
            var newUserModel = new RegisterRequest { UserName = "newuser", Password = "Password123", RoleName = "Tester" };
            _mockRoleManager.Setup(x => x.RoleExistsAsync("Tester")).ReturnsAsync(false);

            // Act
            var result = await service.RegisterAsync(newUserModel);

            // Assert
            // SỬA LỖI KIỂU DỮ LIỆU: RegisterAsync trả về bool, nên Assert là Is.True
            Assert.That(result, Is.True);

            // Xác minh: UserManager.CreateAsync đã được gọi 1 lần
            _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "Password123"), Times.Once);

            // Xác minh: RoleManager.CreateAsync đã được gọi 1 lần
            _mockRoleManager.Verify(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == "Tester")), Times.Once);

            // Xác minh: UserManager.AddToRoleAsync đã được gọi 1 lần
            _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Tester"), Times.Once);
        }

        [Test]
        public async Task RegisterAsync_ShouldReturnFalse_WhenUserCreationFails()
        {
            // Arrange
            var service = CreateService();
            var model = new RegisterRequest { UserName = "failuser", Password = "Pass", RoleName = "EndUser" };

            // Act
            var result = await service.RegisterAsync(model);

            // Assert
            // SỬA LỖI KIỂU DỮ LIỆU: RegisterAsync trả về bool, nên Assert là Is.False
            Assert.That(result, Is.False);

            // Xác minh: Nếu tạo thất bại, AddToRoleAsync KHÔNG được gọi
            _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        // -------------------------------------------------------------------
        // --- TEST CHO ĐĂNG NHẬP (LoginAsync) ---
        // -------------------------------------------------------------------

        [Test]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            var service = CreateService();
            var validModel = new LoginRequest { UserName = "testuser", Password = "validpass" };

            // Act
            var token = await service.LoginAsync(validModel);

            // Assert
            Assert.That(token, Is.Not.Null);
            Assert.That(token.Length, Is.GreaterThan(50));

            // Xác minh: CheckPasswordSignInAsync đã được gọi 1 lần
            _mockSignInManager.Verify(
                x => x.CheckPasswordSignInAsync(_userList.First(), "validpass", false),
                Times.Once);
        }

        [Test]
        public async Task LoginAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var service = CreateService();
            var invalidModel = new LoginRequest { UserName = "nonexistentuser", Password = "anypass" };
            // SỬA: Sử dụng Setup thay vì dùng _mockUserManager.Setup trực tiếp (đã có ở Setup)
            _mockUserManager.Setup(x => x.FindByNameAsync("nonexistentuser")).ReturnsAsync((ApplicationUser)null);

            // Act
            var token = await service.LoginAsync(invalidModel);

            // Assert
            Assert.That(token, Is.Null);

            // Xác minh: CheckPasswordSignInAsync KHÔNG được gọi
            _mockSignInManager.Verify(
                x => x.CheckPasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);
        }

        [Test]
        public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsIncorrect()
        {
            // Arrange
            var service = CreateService();
            var invalidModel = new LoginRequest { UserName = "testuser", Password = "wrongpass" };
            _mockUserManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(_userList.First());

            // Act
            var token = await service.LoginAsync(invalidModel);

            // Assert
            Assert.That(token, Is.Null);
        }
    }
}