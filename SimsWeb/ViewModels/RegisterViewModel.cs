using System.ComponentModel.DataAnnotations;

namespace SimsWeb.ViewModels
{
    public class RegisterViewModel
    {

        [Required(ErrorMessage = "Username is required.")]

        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]

        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength (40, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword", ErrorMessage = "Password does not match.")]

        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Confirmation password does not match.")]

        public string ConfirmPassword { get; set; }
    }
}
