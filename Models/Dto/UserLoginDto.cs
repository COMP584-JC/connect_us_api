using System.ComponentModel.DataAnnotations;

namespace connect_us_api.Models.Dto
{
    public class UserLoginDto
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;
    }
} 