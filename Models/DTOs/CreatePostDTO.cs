using System.ComponentModel.DataAnnotations;

namespace connect_us_api.Models.DTOs
{
    public class CreatePostDTO
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(40, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 40 characters.")]
        public required string Title { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Content must be between 1 and 1000 characters.")]
        public required string Content { get; set; }
    }
} 