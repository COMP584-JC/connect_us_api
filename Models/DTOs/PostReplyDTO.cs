using System.ComponentModel.DataAnnotations;

namespace connect_us_api.Models.DTOs
{
    public class PostReplyDTO
    {
        public long PostReplyId { get; set; }
        public long PostId { get; set; }
        public long? ParentId { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Reply { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<PostReplyDTO>? Children { get; set; }
    }
} 