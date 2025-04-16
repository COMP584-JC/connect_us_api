using System.ComponentModel.DataAnnotations;

namespace connect_us_api.Models.DTOs
{
    public class CreatePostDTO
    {
        [Required(ErrorMessage = "제목을 입력해주세요.")]
        [StringLength(40, MinimumLength = 1, ErrorMessage = "제목은 1자 이상 40자 이하로 입력해주세요.")]
        public required string Title { get; set; }

        [Required(ErrorMessage = "내용을 입력해주세요.")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "내용은 1자 이상 1000자 이하로 입력해주세요.")]
        public required string Content { get; set; }
    }
} 