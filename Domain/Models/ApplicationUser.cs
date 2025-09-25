using Microsoft.AspNetCore.Identity;

namespace Domain.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Thêm các thuộc tính tùy chỉnh ở đây
        public string? UserType { get; set; }
    }
}