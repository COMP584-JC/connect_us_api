using System.ComponentModel.DataAnnotations;

namespace connect_us_api.Models.DTOs
{
    public class UserRegistrationDTO
    {
        [Required(ErrorMessage = "이름을 입력해주세요.")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "아이디를 입력해주세요.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "아이디는 3자 이상 50자 이하로 입력해주세요.")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "이메일을 입력해주세요.")]
        [EmailAddress(ErrorMessage = "올바른 이메일 형식이 아닙니다.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "비밀번호를 입력해주세요.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "비밀번호는 6자 이상 100자 이하로 입력해주세요.")]
        public required string Password { get; set; }
    }
} 