using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace connect_us_api.Models
{
    public class PostReply
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PostReplyId { get; set; } 

        public long PostId { get; set; }
        public required Post Post { get; set; }

        public long? ParentId { get; set; }
        public PostReply? Parent { get; set; }

        [NotMapped]
        public List<PostReply>? Children { get; set; }

        public long UserId { get; set; }
        public required User User { get; set; }

        [Required]
        public required string Reply { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
