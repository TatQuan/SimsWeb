using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SimsWeb.ViewModels
{
    public class UserEditViewModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        // Role data
        public string SelectedRole { get; set; }   // Role hiện tại
        public List<string> RoleList { get; set; } = new(); // Dropdown
    }
}
