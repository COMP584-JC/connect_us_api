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
            context.Database.EnsureCreated();

            if (context.Users.Any())
            {
                return;
            }

            var users = new User[]
            {
                CreateUser("John Doe", "johndoe", "john@example.com", "password123"),
                CreateUser("Jane Smith", "janesmith", "jane@example.com", "password123"),
                CreateUser("Bob Wilson", "bobwilson", "bob@example.com", "password123"),
                CreateUser("Alice Johnson", "alicej", "alice@example.com", "password123"),
                CreateUser("Charlie Brown", "charlieb", "charlie@example.com", "password123")
            };

            foreach (User u in users)
            {
                context.Users.Add(u);
            }
            context.SaveChanges();

            var posts = new Post[]
            {
                new Post
                {
                    Title = "First Post",
                    Content = "Hello! This is the first post.",
                    UserId = users[0].UserId,
                    User = users[0]
                },
                new Post
                {
                    Title = "Second Post",
                    Content = "This is the second post. How are you all doing?",
                    UserId = users[1].UserId,
                    User = users[1]
                }
            };

            context.Posts.AddRange(posts);
            context.SaveChanges();

            var replies = new List<PostReply>();

            var firstPostMainReply = new PostReply
            {
                PostId = posts[0].PostId,
                Post = posts[0],
                UserId = users[1].UserId,
                User = users[1],
                Reply = "Good post!"
            };
            replies.Add(firstPostMainReply);

            replies.Add(new PostReply
            {
                PostId = posts[0].PostId,
                Post = posts[0],
                UserId = users[2].UserId,
                User = users[2],
                Reply = "Yes, it's a great post!",
                ParentId = firstPostMainReply.PostReplyId,
                Parent = firstPostMainReply
            });

            replies.Add(new PostReply
            {
                PostId = posts[0].PostId,
                Post = posts[0],
                UserId = users[3].UserId,
                User = users[3],
                Reply = "I agree!",
                ParentId = firstPostMainReply.PostReplyId,
                Parent = firstPostMainReply
            });

            var secondPostMainReply = new PostReply
            {
                PostId = posts[1].PostId,
                Post = posts[1],
                UserId = users[0].UserId,
                User = users[0],
                Reply = "Yes, I'm doing well!"
            };
            replies.Add(secondPostMainReply);

            var secondPostNestedReply = new PostReply
            {
                PostId = posts[1].PostId,
                Post = posts[1],
                UserId = users[2].UserId,
                User = users[2],
                Reply = "I'm also doing well!",
                ParentId = secondPostMainReply.PostReplyId,
                Parent = secondPostMainReply
            };
            replies.Add(secondPostNestedReply);

            replies.Add(new PostReply
            {
                PostId = posts[1].PostId,
                Post = posts[1],
                UserId = users[4].UserId,
                User = users[4],
                Reply = "Good news!",
                ParentId = secondPostNestedReply.PostReplyId,
                Parent = secondPostNestedReply
            });

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