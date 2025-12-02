using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SimsWeb.ViewModels
{
    public class AdminCreateUserViewModel : RegisterViewModel
    {
        // Role được chọn bên dropdown
        public string SelectedRole { get; set; }

        // Danh sách role hiển thị dropdown
        public List<string> RoleList { get; set; } = new();
    }
}
