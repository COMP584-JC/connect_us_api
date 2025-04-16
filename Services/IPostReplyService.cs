using connect_us_api.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace connect_us_api.Services
{
    public interface IPostReplyService
    {
        Task<IEnumerable<PostReplyDTO>> GetRepliesByPostIdAsync(long postId);
        Task<PostReplyDTO> GetReplyByIdAsync(long replyId);
    }
} 