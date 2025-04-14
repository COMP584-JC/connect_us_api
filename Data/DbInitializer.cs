using connect_us_api.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace connect_us_api.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Check if database is created
            context.Database.EnsureCreated();

            // Check if data already exists
            if (context.Users.Any())
            {
                return; // DB is already seeded
            }

            // Create sample users
            var users = new User[]
            {
                CreateUser("John Doe", "johndoe", "john@example.com", "password123"),
                CreateUser("Jane Smith", "janesmith", "jane@example.com", "password123"),
                CreateUser("Bob Wilson", "bobwilson", "bob@example.com", "password123")
            };

            foreach (User u in users)
            {
                context.Users.Add(u);
            }
            context.SaveChanges();

            // Create sample posts
            var posts = new Post[]
            {
                new Post
                {
                    Title = "First Post",
                    Content = "Hello everyone! This is the content of my first post.",
                    UserId = users[0].UserId,
                    User = users[0]
                },
                new Post
                {
                    Title = "Second Post",
                    Content = "This is the content of my second post. Hello everyone!",
                    UserId = users[1].UserId,
                    User = users[1]
                },
                new Post
                {
                    Title = "Third Post",
                    Content = "This is my third post. The weather is nice today!",
                    UserId = users[2].UserId,
                    User = users[2]
                }
            };

            context.Posts.AddRange(posts);
            context.SaveChanges();

            // Create sample replies
            var replies = new PostReply[]
            {
                new PostReply
                {
                    PostId = posts[0].PostId,
                    Post = posts[0],
                    UserId = users[1].UserId,
                    User = users[1],
                    Reply = "Great post!"
                },
                new PostReply
                {
                    PostId = posts[0].PostId,
                    Post = posts[0],
                    UserId = users[2].UserId,
                    User = users[2],
                    Reply = "Thank you!",
                    ParentId = 1 // Reply to the first comment
                },
                new PostReply
                {
                    PostId = posts[1].PostId,
                    Post = posts[1],
                    UserId = users[0].UserId,
                    User = users[0],
                    Reply = "This is a comment on the second post."
                }
            };

            context.PostReplies.AddRange(replies);
            context.SaveChanges();
        }

        private static User CreateUser(string name, string username, string email, string password)
        {
            using var hmac = new HMACSHA512();
            var salt = Convert.ToBase64String(hmac.Key);
            var hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));

            return new User
            {
                Name = name,
                Username = username,
                Email = email,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
} 