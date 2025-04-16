using connect_us_api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace connect_us_api.Services
{
    public interface IPostService
    {
        Task<IEnumerable<Post>> GetAllPostsAsync();
        Task<Post> GetPostByIdAsync(long id);
    }
} 