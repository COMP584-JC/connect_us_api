using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using connect_us_api.Models;
using connect_us_api.Models.DTOs;
using connect_us_api.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using System.Text;
using System;

namespace connect_us_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IPostReplyService _postReplyService;
        private readonly IConfiguration _configuration;

        public PostController(IPostService postService, IPostReplyService postReplyService, IConfiguration configuration)
        {
            _postService = postService;
            _postReplyService = postReplyService;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetAllPosts([FromQuery] string? search)
        {
            var posts = await _postService.GetAllPostsAsync();
            
            if (!string.IsNullOrWhiteSpace(search))
            {
                posts = posts.Where(p => p.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            
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

        [HttpGet("replies/{id}")]
        public async Task<ActionResult<PostReplyDTO>> GetReplyById(long id)
        {
            var reply = await _postReplyService.GetReplyByIdAsync(id);
            if (reply == null)
            {
                return NotFound();
            }
            return Ok(reply);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Post>> CreatePost([FromBody] CreatePostDTO postDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
                {
                    return Unauthorized(new { message = "Unknown user" });
                }

                if (string.IsNullOrWhiteSpace(postDto.Title))
                {
                    return BadRequest(new { message = "Title is required" });
                }

                if (string.IsNullOrWhiteSpace(postDto.Content))
                {
                    return BadRequest(new { message = "Content is required" });
                }

                if (postDto.Title.Length > 40)
                {
                    return BadRequest(new { message = "Title cannot exceed 40 characters" });
                }

                if (postDto.Content.Length > 1000)
                {
                    return BadRequest(new { message = "Content cannot exceed 1000 characters" });
                }

                var post = await _postService.CreatePostAsync(postDto, userId);
                return CreatedAtAction(nameof(GetPostById), new { id = post.PostId }, post);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error occurred while creating post. Please try again later." });
            }
        }

        [Authorize]
        [HttpPost("{postId}/replies")]
        public async Task<ActionResult<PostReplyDTO>> CreatePostReply(long postId, [FromBody] CreatePostReplyDTO replyDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
                {
                    return Unauthorized(new { message = "Invalid user information" });
                }

                if (string.IsNullOrWhiteSpace(replyDto.Reply))
                {
                    return BadRequest(new { message = "Reply content is required" });
                }

                if (replyDto.Reply.Length > 500)
                {
                    return BadRequest(new { message = "Reply cannot exceed 500 characters" });
                }

                var post = await _postService.GetPostByIdAsync(postId);
                if (post == null)
                {
                    return NotFound(new { message = "Post not found" });
                }

                replyDto.ParentId = null;
                var reply = await _postReplyService.CreateReplyAsync(replyDto, postId, userId);
                if (reply == null)
                {
                    return StatusCode(500, new { message = "Error occurred while creating reply. Please try again later." });
                }
                    
                return Ok(reply);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error occurred while creating reply. Please try again later." });
            }
        }

        [Authorize]
        [HttpPost("replies/{replyId}/reply")]
        public async Task<ActionResult<PostReplyDTO>> CreateReplyToReply(long replyId, [FromBody] CreatePostReplyDTO replyDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
                {
                    return Unauthorized(new { message = "Invalid user information" });
                }

                if (string.IsNullOrWhiteSpace(replyDto.Reply))
                {
                    return BadRequest(new { message = "Reply content is required" });
                }

                if (replyDto.Reply.Length > 500)
                {
                    return BadRequest(new { message = "Reply cannot exceed 500 characters" });
                }

                var parentReply = await _postReplyService.GetReplyByIdAsync(replyId);
                if (parentReply == null)
                {
                    return NotFound(new { message = "Original reply not found" });
                }

                var post = await _postService.GetPostByIdAsync(parentReply.PostId);
                if (post == null)
                {
                    return NotFound(new { message = "Post not found" });
                }

                replyDto.ParentId = replyId;
                var reply = await _postReplyService.CreateReplyAsync(replyDto, parentReply.PostId, userId);
                if (reply == null)
                {
                    return StatusCode(500, new { message = "Error occurred while creating reply. Please try again later." });
                }
                    
                return Ok(reply);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error occurred while creating reply. Please try again later." });
            }
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