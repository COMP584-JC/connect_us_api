using connect_us_api.Models;
using connect_us_api.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace connect_us_api.Services
{
    public interface IPostService
    {
        Task<IEnumerable<Post>> GetAllPostsAsync();
        Task<Post> GetPostByIdAsync(long id);
        Task<Post> CreatePostAsync(CreatePostDTO postDto, long userId);
    }
} 