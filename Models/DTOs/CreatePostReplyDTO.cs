using System.ComponentModel.DataAnnotations;

namespace connect_us_api.Models.DTOs
{
    public class CreatePostReplyDTO
    {
        [Required(ErrorMessage = "Reply content is required.")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Reply must be between 1 and 1000 characters.")]
        public required string Reply { get; set; }

        public long? ParentId { get; set; }
    }
} 