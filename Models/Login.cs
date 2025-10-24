using System.ComponentModel.DataAnnotations;

namespace POE_MVC_part1.Models
{
    public class Login
    {

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string email { get; set; }


        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
           ErrorMessage = "Password must include uppercase, lowercase, number, and special character, and be at least 8 characters.")]
        public string password { get; set; }


        [Required(ErrorMessage = "Please select your role.")]
        public string Role { get; set; }
    }
}
