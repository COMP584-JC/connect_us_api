using System.ComponentModel.DataAnnotations;

namespace connect_us_api.Models.DTOs
{
    public class CreatePostReplyDTO
    {
        [Required(ErrorMessage = "댓글 내용을 입력해주세요.")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "댓글은 1자 이상 1000자 이하로 입력해주세요.")]
        public required string Reply { get; set; }

        public long? ParentId { get; set; }
    }
} 