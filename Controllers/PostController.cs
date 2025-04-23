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
                    return Unauthorized(new { message = "유효하지 않은 사용자 정보입니다." });
                }

                // 입력값 유효성 검사
                if (string.IsNullOrWhiteSpace(postDto.Title))
                {
                    return BadRequest(new { message = "제목을 입력해주세요." });
                }

                if (string.IsNullOrWhiteSpace(postDto.Content))
                {
                    return BadRequest(new { message = "내용을 입력해주세요." });
                }

                if (postDto.Title.Length > 40)
                {
                    return BadRequest(new { message = "제목은 40자를 초과할 수 없습니다." });
                }

                if (postDto.Content.Length > 1000)
                {
                    return BadRequest(new { message = "내용은 1000자를 초과할 수 없습니다." });
                }

                var post = await _postService.CreatePostAsync(postDto, userId);
                return CreatedAtAction(nameof(GetPostById), new { id = post.PostId }, post);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "게시글 작성 중 오류가 발생했습니다. 잠시 후 다시 시도해주세요." });
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
                    return Unauthorized(new { message = "유효하지 않은 사용자 정보입니다." });
                }

                // 입력값 유효성 검사
                if (string.IsNullOrWhiteSpace(replyDto.Reply))
                {
                    return BadRequest(new { message = "댓글 내용을 입력해주세요." });
                }

                if (replyDto.Reply.Length > 500)
                {
                    return BadRequest(new { message = "댓글은 500자를 초과할 수 없습니다." });
                }

                // 게시글 존재 여부 확인
                var post = await _postService.GetPostByIdAsync(postId);
                if (post == null)
                {
                    return NotFound(new { message = "게시글을 찾을 수 없습니다." });
                }

                replyDto.ParentId = null; // 게시글에 대한 새 댓글은 항상 parentId가 null
                var reply = await _postReplyService.CreateReplyAsync(replyDto, postId, userId);
                if (reply == null)
                {
                    return StatusCode(500, new { message = "댓글 작성 중 오류가 발생했습니다. 잠시 후 다시 시도해주세요." });
                }
                    
                return Ok(reply);
            }
            catch (Exception ex)
            {
                // 로깅 추가 필요
                return StatusCode(500, new { message = "댓글 작성 중 오류가 발생했습니다. 잠시 후 다시 시도해주세요." });
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
                    return Unauthorized(new { message = "유효하지 않은 사용자 정보입니다." });
                }

                // 입력값 유효성 검사
                if (string.IsNullOrWhiteSpace(replyDto.Reply))
                {
                    return BadRequest(new { message = "댓글 내용을 입력해주세요." });
                }

                if (replyDto.Reply.Length > 500)
                {
                    return BadRequest(new { message = "댓글은 500자를 초과할 수 없습니다." });
                }

                // 부모 댓글 존재 여부 확인
                var parentReply = await _postReplyService.GetReplyByIdAsync(replyId);
                if (parentReply == null)
                {
                    return NotFound(new { message = "원본 댓글을 찾을 수 없습니다." });
                }

                // 부모 댓글의 게시글 존재 여부 확인
                var post = await _postService.GetPostByIdAsync(parentReply.PostId);
                if (post == null)
                {
                    return NotFound(new { message = "게시글을 찾을 수 없습니다." });
                }

                replyDto.ParentId = replyId;
                var reply = await _postReplyService.CreateReplyAsync(replyDto, parentReply.PostId, userId);
                if (reply == null)
                {
                    return StatusCode(500, new { message = "댓글 작성 중 오류가 발생했습니다. 잠시 후 다시 시도해주세요." });
                }
                    
                return Ok(reply);
            }
            catch (Exception ex)
            {
                // 로깅 추가 필요
                return StatusCode(500, new { message = "댓글 작성 중 오류가 발생했습니다. 잠시 후 다시 시도해주세요." });
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