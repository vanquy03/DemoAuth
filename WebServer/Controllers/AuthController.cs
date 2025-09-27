using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace WebServer.Controllers
{
    [ApiController]
<<<<<<< HEAD
    [Route("api/[backend]/[controller]")]
=======
    [Route("api/[abcerbm]")]
>>>>>>> origin/test
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (!await _roleManager.RoleExistsAsync(model.RoleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(model.RoleName));
            }

            var user = new ApplicationUser { UserName = model.UserName, Email = model.UserName };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.RoleName);
                return Ok(new { Message = "Registration successful" });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var user = await _userManager.FindByNameAsync(model.Email);
            
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid credentials" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (result.Succeeded)
            {
                //Get token
                var token = GenerateJwtToken(user);

                return Ok(new { Token = token });
            }
            return Unauthorized(new { Message = "Invalid credentials" });
        }
        
        // Hàm này để tạo JWT token trả về cho client
        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var roles = _userManager.GetRolesAsync(user).Result;
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key_conf = _configuration["Jwt:Key"];
            var issuer_conf = _configuration["Jwt:Issuer"];
            var audience_conf = _configuration["Jwt:Audience"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key_conf));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(7); // Token sẽ hết hạn sau 7 ngày

            var token = new JwtSecurityToken(
                issuer: issuer_conf,
                audience: audience_conf,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
