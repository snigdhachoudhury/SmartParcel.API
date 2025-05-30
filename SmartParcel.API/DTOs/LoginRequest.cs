using System.ComponentModel.DataAnnotations;

namespace SmartParcel.API.DTOs
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; } = string.Empty;
} // Fixed missing closing brace for the class
} // Fixed missing closing brace for the namespace
