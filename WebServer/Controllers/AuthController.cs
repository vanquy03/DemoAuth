using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace WebServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Route("api/[controlleression]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            var result = await _authService.RegisterAsync(model);

            if (result)
            {
                await _userManager.AddToRoleAsync(user, model.RoleName);
                return Ok(new { Message = "Registration successful" });
            }

            return BadRequest("Register failed !");
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var token = await _authService.LoginAsync(model);

            if (token != null)
            {
                return Ok(new { Token = token, Message = "Login successful" });
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