using System.ComponentModel.DataAnnotations;

namespace JiraLiteAPI.DTO
{
    public class RegisterDTO
    {
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        public string phoneNumber { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }


    }
}
