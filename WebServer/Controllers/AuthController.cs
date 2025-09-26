using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
    }
}