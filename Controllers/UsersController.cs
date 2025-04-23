using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using connect_us_api.Models;
using connect_us_api.Models.DTOs;
using connect_us_api.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace connect_us_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<UsersController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDTO userDto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == userDto.Username))
                return BadRequest(new { message = "Username already in use." });

            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
                return BadRequest(new { message = "Email already in use." });

            using var hmac = new HMACSHA512();
            var salt = Convert.ToBase64String(hmac.Key);
            var hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(userDto.Password)));

            var user = new User
            {
                Name         = userDto.Name,
                Username     = userDto.Username,
                Email        = userDto.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt    = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Registration successful." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null)
                return Unauthorized(new { message = "Invalid username or password." });

            using var hmac = new HMACSHA512(Convert.FromBase64String(user.PasswordSalt));
            var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password)));
            if (user.PasswordHash != computedHash)
                return Unauthorized(new { message = "Invalid username or password." });

            var token = GenerateJwtToken(user);
            return Ok(new
            {
                message = "Login successful",
                token,
                user = new
                {
                    userId   = user.UserId,
                    name     = user.Name,
                    username = user.Username,
                    email    = user.Email
                }
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            _logger.LogInformation("User logged out.");
            return Ok(new { message = "Logged out successfully." });
        }

        [HttpGet("check-auth")]
        public IActionResult CheckAuth()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (!authHeader.StartsWith("Bearer ")) 
                return Unauthorized(new { isAuthenticated = false });

            var token = authHeader.Substring("Bearer ".Length).Trim();
            try
            {
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
                var principal = new JwtSecurityTokenHandler().ValidateToken(
                    token,
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey         = new SymmetricSecurityKey(key),
                        ValidateIssuer           = false,
                        ValidateAudience         = false,
                        ValidateLifetime         = true,
                        ClockSkew                = TimeSpan.Zero
                    },
                    out _);

                return Ok(new { isAuthenticated = principal.Identity?.IsAuthenticated == true });
            }
            catch
            {
                return Unauthorized(new { isAuthenticated = false });
            }
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name,           user.Username),
                new Claim(ClaimTypes.Email,          user.Email)
            };

            var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token  = new JwtSecurityToken(
                issuer:    _configuration["Jwt:Issuer"],
                audience:  _configuration["Jwt:Audience"],
                claims:    claims,
                expires:   DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
