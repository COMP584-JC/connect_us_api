using System.ComponentModel.DataAnnotations;

namespace connect_us_api.Models.DTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Username is required.")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public required string Password { get; set; }
    }
} 