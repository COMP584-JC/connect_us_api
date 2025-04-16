using Microsoft.AspNetCore.Mvc;
using connect_us_api.Models;
using connect_us_api.Models.DTOs;
using connect_us_api.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace connect_us_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IPostReplyService _postReplyService;

        public PostController(IPostService postService, IPostReplyService postReplyService)
        {
            _postService = postService;
            _postReplyService = postReplyService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetAllPosts()
        {
            var posts = await _postService.GetAllPostsAsync();
            return Ok(posts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPostById(long id)
        {
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            return Ok(post);
        }

        [HttpGet("{postId}/replies")]
        public async Task<ActionResult<IEnumerable<PostReplyDTO>>> GetPostReplies(long postId)
        {
            var replies = await _postReplyService.GetRepliesByPostIdAsync(postId);
            return Ok(replies);
        }

        [HttpGet("replies/{replyId}")]
        public async Task<ActionResult<PostReplyDTO>> GetReplyById(long replyId)
        {
            var reply = await _postReplyService.GetReplyByIdAsync(replyId);
            if (reply == null)
            {
                return NotFound();
            }
            return Ok(reply);
        }

        private List<PostReply> BuildReplyTree(IEnumerable<PostReply> replies)
        {
            var replyLookup = replies.ToDictionary(r => r.PostReplyId);
            var rootReplies = new List<PostReply>();

            foreach (var reply in replies)
            {
                if (reply.ParentId == null)
                {
                    rootReplies.Add(reply);
                }
                else if (replyLookup.TryGetValue(reply.ParentId.Value, out var parent))
                {
                    if (parent.Children == null)
                    {
                        parent.Children = new List<PostReply>();
                    }
                    parent.Children.Add(reply);
                }
            }

            return rootReplies;
        }
    }
} 