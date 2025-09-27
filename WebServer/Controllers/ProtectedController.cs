using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProtectedController : ControllerBase
{
    [HttpGet]
    [Authorize]
    public IActionResult GetProtectedData()
    {
        return Ok(new { message = "Đây là dữ liệu bí mật chỉ dành cho người dùng đã đăng nhập!" });
    }
}