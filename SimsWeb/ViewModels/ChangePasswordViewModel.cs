using System.ComponentModel.DataAnnotations;

namespace SimsWeb.ViewModels
{
    public class ChangePasswordViewModel
    {

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress] 

        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "The {0} must be at {2} and max {1} characters long.")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        [Compare("ConfirmPassword", ErrorMessage = "Password does not match.")]

        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "Confirmation password does not match.")]

        public string ConfirmPassword { get; set; }
    }
}
