using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using connect_us_api.Models;
using connect_us_api.Data;
using System.Security.Cryptography;
using System.Text;

namespace connect_us_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistrationDto userDto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == userDto.Username))
            {
                return BadRequest("Already used username");
            }

            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
            {
                return BadRequest("Already used email");
            }

            using var hmac = new HMACSHA512();
            var user = new User
            {
                Name = userDto.Username,
                Username = userDto.Username,
                Email = userDto.Email,
                PasswordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(userDto.Password)))
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Signup completed" });
        }
    }
}
