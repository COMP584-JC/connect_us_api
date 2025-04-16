using connect_us_api.Models;
using connect_us_api.Models.DTOs;
using connect_us_api.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace connect_us_api.Services
{
    public class PostReplyService : IPostReplyService
    {
        private readonly ApplicationDbContext _context;

        public PostReplyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PostReplyDTO>> GetRepliesByPostIdAsync(long postId)
        {
            var replies = await _context.PostReplies
                .Include(r => r.User)
                .Where(r => r.PostId == postId)
                .ToListAsync();

            return BuildReplyTree(replies);
        }

        public async Task<PostReplyDTO> GetReplyByIdAsync(long replyId)
        {
            var reply = await _context.PostReplies
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.PostReplyId == replyId);

            if (reply == null)
                return null;

            return new PostReplyDTO
            {
                PostReplyId = reply.PostReplyId,
                PostId = reply.PostId,
                ParentId = reply.ParentId,
                UserId = reply.UserId,
                UserName = reply.User.Name,
                Reply = reply.Reply,
                CreatedAt = reply.CreatedAt,
                UpdatedAt = reply.UpdatedAt
            };
        }

        private List<PostReplyDTO> BuildReplyTree(IEnumerable<PostReply> replies, long? parentId = null)
        {
            return replies
                .Where(r => r.ParentId == parentId)
                .Select(r => new PostReplyDTO
                {
                    PostReplyId = r.PostReplyId,
                    PostId = r.PostId,
                    ParentId = r.ParentId,
                    UserId = r.UserId,
                    UserName = r.User.Name,
                    Reply = r.Reply,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    Children = BuildReplyTree(replies, r.PostReplyId)
                })
                .ToList();
        }
    }
} 