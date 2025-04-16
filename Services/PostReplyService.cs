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
                .Include(r => r.Parent)
                    .ThenInclude(p => p.User)
                .Include(r => r.Post)
                    .ThenInclude(p => p.User)
                .Where(r => r.PostId == postId)
                .ToListAsync();

            return BuildReplyTree(replies);
        }

        public async Task<PostReplyDTO> GetReplyByIdAsync(long replyId)
        {
            var reply = await _context.PostReplies
                .Include(r => r.User)
                .Include(r => r.Parent)
                    .ThenInclude(p => p.User)
                .Include(r => r.Post)
                    .ThenInclude(p => p.User)
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
                UpdatedAt = reply.UpdatedAt,
                Children = new List<PostReplyDTO>()
            };
        }

        public async Task<PostReplyDTO> CreateReplyAsync(CreatePostReplyDTO replyDto, long postId, long userId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
                return null;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return null;

            var reply = new PostReply
            {
                PostId = postId,
                ParentId = replyDto.ParentId,
                UserId = userId,
                Reply = replyDto.Reply,
                Post = post,
                User = user
            };

            _context.PostReplies.Add(reply);
            await _context.SaveChangesAsync();

            return await GetReplyByIdAsync(reply.PostReplyId);
        }

        private IEnumerable<PostReplyDTO> BuildReplyTree(List<PostReply> replies)
        {
            var replyDict = replies.ToDictionary(r => r.PostReplyId);
            var rootReplies = new List<PostReplyDTO>();

            foreach (var reply in replies)
            {
                var dto = new PostReplyDTO
                {
                    PostReplyId = reply.PostReplyId,
                    PostId = reply.PostId,
                    ParentId = reply.ParentId,
                    UserId = reply.UserId,
                    UserName = reply.User.Name,
                    Reply = reply.Reply,
                    CreatedAt = reply.CreatedAt,
                    UpdatedAt = reply.UpdatedAt,
                    Children = new List<PostReplyDTO>()
                };

                if (reply.ParentId == null)
                {
                    rootReplies.Add(dto);
                }
                else if (replyDict.TryGetValue(reply.ParentId.Value, out var parent))
                {
                    var parentDto = rootReplies.FirstOrDefault(r => r.PostReplyId == parent.PostReplyId);
                    if (parentDto != null)
                    {
                        parentDto.Children.Add(dto);
                    }
                }
            }

            return rootReplies;
        }
    }
} 