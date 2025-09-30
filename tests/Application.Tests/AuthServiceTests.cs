using Application.Dtos;
using Application.Services;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Tests
{
    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private Mock<IConfiguration> _configurationMock;
        private AuthService _authService;

        [SetUp]
        public void Setup()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                roleStore.Object, null, null, null, null);

            var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object, contextAccessor.Object,
                userPrincipalFactory.Object, null, null, null, null);

            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c["Jwt:Key"]).Returns("VGhpc2Q5/3MXg7mWvA38i6Ch/ZqzCeAt3NrvfVqw6G58cevKf+jwCuohK67/I2YqwWfprJKx7KO8mKWQ6/4868mCfIw==0lzQVZlcnlTdHJvbmdBbmRTZWN1cmFLZXlGb3JZb3Vyaml3dFRva2VuQXV0aGVudGljYXRpb24");
            _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("http://localhost:5280");
            _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("my-web_client");
            _authService = new AuthService(
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _signInManagerMock.Object,
                _configurationMock.Object
            );
        }

        [Test]
        public async Task RegisterAsync_ShouldReturnTrue_WhenUserCreatedSuccessfully()
        {
            var request = new RegisterRequest { UserName = "testuser", Password = "Pass@123", RoleName = "Admin" };

            _roleManagerMock.Setup(r => r.RoleExistsAsync(request.RoleName)).ReturnsAsync(false);
            _roleManagerMock.Setup(r => r.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), request.RoleName))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _authService.RegisterAsync(request);

            Assert.IsTrue(result);
        }

        //[Test]
        //public async Task RegisterAsync_ShouldReturnFalse_WhenUserCreationFails()
        //{
        //    var request = new RegisterRequest { UserName = "testuser", Password = "Pass@123", RoleName = "User" };

        //    _roleManagerMock.Setup(r => r.RoleExistsAsync(request.RoleName)).ReturnsAsync(true);
        //    _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
        //        .ReturnsAsync(IdentityResult.Failed());

        //    var result = await _authService.RegisterAsync(request);

        //    Assert.IsFalse(result);
        //}

        //[Test]
        //public async Task LoginAsync_ShouldReturnNull_WhenUserNotFound()
        //{
        //    var request = new LoginRequest { UserName = "notfound", Password = "wrong" };
        //    _userManagerMock.Setup(u => u.FindByNameAsync(request.UserName)).ReturnsAsync((ApplicationUser)null);

        //    var result = await _authService.LoginAsync(request);

        //    Assert.IsNull(result);
        //}

        //[Test]
        //public async Task LoginAsync_ShouldReturnNull_WhenPasswordInvalid()
        //{
        //    var request = new LoginRequest { UserName = "testuser", Password = "wrong" };
        //    var user = new ApplicationUser { UserName = request.UserName };

        //    _userManagerMock.Setup(u => u.FindByNameAsync(request.UserName)).ReturnsAsync(user);
        //    _signInManagerMock.Setup(s => s.CheckPasswordSignInAsync(user, request.Password, false))
        //        .ReturnsAsync(SignInResult.Failed);

        //    var result = await _authService.LoginAsync(request);

        //    Assert.IsNull(result);
        //}

        //[Test]
        //public async Task LoginAsync_ShouldReturnToken_WhenLoginSuccessful()
        //{
        //    var request = new LoginRequest { UserName = "testuser", Password = "Pass@123" };
        //    var user = new ApplicationUser { Id = "1", UserName = request.UserName, Email = "test@test.com" };

        //    _userManagerMock.Setup(u => u.FindByNameAsync(request.UserName)).ReturnsAsync(user);
        //    _signInManagerMock.Setup(s => s.CheckPasswordSignInAsync(user, request.Password, false))
        //        .ReturnsAsync(SignInResult.Success);
        //    _userManagerMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });

        //    var result = await _authService.LoginAsync(request);

        //    Assert.IsNotNull(result);
        //    Assert.IsTrue(result.Length > 20); // token string length check
        //}
    }
}
