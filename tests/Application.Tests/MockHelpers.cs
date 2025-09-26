using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
namespace Application.Tests
{
    public static class MockHelpers
    {
        public static Mock<UserManager<TUser>> MockUserManager<TUser>(List<TUser> userList) where TUser : ApplicationUser
        {
            var store = new Mock<IUserStore<TUser>>();

            var mgr = new Mock<UserManager<TUser>>(
                store.Object, // 1. IUserStore<TUser>
                new Mock<IOptions<IdentityOptions>>().Object, // 2. IOptions<IdentityOptions>
                new Mock<IPasswordHasher<TUser>>().Object,   // 3. IPasswordHasher<TUser>
                new List<IUserValidator<TUser>>().AsEnumerable(), // 4. IEnumerable<IUserValidator<TUser>>
                new List<IPasswordValidator<TUser>>().AsEnumerable(), // 5. IEnumerable<IPasswordValidator<TUser>>
                new Mock<ILookupNormalizer>().Object,        // 6. ILookupNormalizer
                new Mock<IdentityErrorDescriber>().Object,   // 7. IdentityErrorDescriber
                new Mock<IServiceProvider>().Object,         // 8. IServiceProvider
                new Mock<ILogger<UserManager<TUser>>>().Object // 9. ILogger<UserManager<TUser>>
            );

            // --- Thiết lập hành vi Mock (Setup) ---
            mgr.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
               .ReturnsAsync((string username) => userList.FirstOrDefault(u => u.UserName == username));

            mgr.Setup(x => x.CreateAsync(It.IsAny<TUser>(), It.IsAny<string>()))
               .ReturnsAsync(IdentityResult.Success);

            mgr.Setup(x => x.CreateAsync(It.Is<TUser>(u => u.UserName == "failuser"), It.IsAny<string>()))
               .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Creation failed." }));

            mgr.Setup(x => x.AddToRoleAsync(It.IsAny<TUser>(), It.IsAny<string>()))
               .ReturnsAsync(IdentityResult.Success);

            mgr.Setup(x => x.GetRolesAsync(It.IsAny<TUser>()))
               .ReturnsAsync(new List<string> { "EndUser" });

            return mgr;
        }

        // Tạo Mock cho RoleManager
        public static Mock<RoleManager<TRole>> MockRoleManager<TRole>(List<TRole> roleList) where TRole : IdentityRole
        {
            var store = new Mock<IRoleStore<TRole>>();
            // Cần truyền store.Object và đủ 5 tham số
            var mgr = new Mock<RoleManager<TRole>>(store.Object, null, null, null, null);

            mgr.Setup(x => x.RoleExistsAsync(It.IsAny<string>()))
               .ReturnsAsync((string roleName) => roleList.Any(r => r.Name == roleName));

            mgr.Setup(x => x.CreateAsync(It.IsAny<TRole>()))
               .ReturnsAsync(IdentityResult.Success);

            return mgr;
        }

        // Tạo Mock cho SignInManager
        public static Mock<SignInManager<TUser>> MockSignInManager<TUser>() where TUser : ApplicationUser
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<TUser>>();
            var options = new Mock<IOptions<IdentityOptions>>();
            var logger = new Mock<ILogger<SignInManager<TUser>>>();
            var userStore = new Mock<IUserStore<TUser>>();

            // Khởi tạo một Mock UserManager đơn giản chỉ để thỏa mãn phụ thuộc của SignInManager
            var userManager = new Mock<UserManager<TUser>>(userStore.Object, null, null, null, null, null, null, null, null).Object;

            var signInManager = new Mock<SignInManager<TUser>>(
                userManager, contextAccessor.Object, claimsFactory.Object,
                options.Object, logger.Object, null, null); // Truyền đủ 7 tham số

            signInManager.Setup(x => x.CheckPasswordSignInAsync(It.IsAny<TUser>(), "validpass", false))
                .ReturnsAsync(SignInResult.Success);

            signInManager.Setup(x => x.CheckPasswordSignInAsync(It.IsAny<TUser>(), "wrongpass", false))
                .ReturnsAsync(SignInResult.Failed);

            return signInManager;
        }

        // Tạo Mock cho IConfiguration (Sử dụng Key JWT của bạn)
        public static Mock<IConfiguration> MockConfiguration()
        {
            var config = new Mock<IConfiguration>();

            // SỬ DỤNG KEY BẠN ĐÃ CUNG CẤP
            config.Setup(c => c["Jwt:Key"]).Returns("VGhpc2Q5/3MXg7mWvA38i6Ch/ZqzCeAt3NrvfVqw6G58cevKf+jwCuohK67/I2YqwWfprJKx7KO8mKWQ6/4868mCfIw==0lzQVZlcnlTdHJvbmdBbmRTZWN1cmFLZXlGb3JZb3Vyaml3dFRva2VuQXV0aGVudGljYXRpb24");
            config.Setup(c => c["Jwt:Issuer"]).Returns("http://localhost:5280");
            config.Setup(c => c["Jwt:Audience"]).Returns("my-web_client");

            return config;
        }
    }
}
// Đảm bảo TUser kế thừa từ ApplicationUser và TRole kế thừa từ IdentityRole
