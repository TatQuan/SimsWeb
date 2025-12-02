using Microsoft.AspNetCore.Identity;

namespace SimsWeb.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; }

        //Dùng để xóa mền
        public bool IsDeleted { get; set; } = false;

        // Thời gian tạo tài khoản
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
