using System.ComponentModel.DataAnnotations;

namespace connect_us_api.Models.DTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "아이디를 입력해주세요.")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "비밀번호를 입력해주세요.")]
        public required string Password { get; set; }
    }
} 