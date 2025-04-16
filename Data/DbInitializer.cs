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
                CreateUser("Bob Wilson", "bobwilson", "bob@example.com", "password123"),
                CreateUser("Alice Johnson", "alicej", "alice@example.com", "password123"),
                CreateUser("Charlie Brown", "charlieb", "charlie@example.com", "password123")
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
                    Title = "첫 번째 포스트",
                    Content = "안녕하세요! 이것은 첫 번째 포스트입니다.",
                    UserId = users[0].UserId,
                    User = users[0]
                },
                new Post
                {
                    Title = "두 번째 포스트",
                    Content = "이것은 두 번째 포스트입니다. 모두들 잘 지내시나요?",
                    UserId = users[1].UserId,
                    User = users[1]
                }
            };

            context.Posts.AddRange(posts);
            context.SaveChanges();

            // Create sample replies with nested replies
            var replies = new List<PostReply>();

            // 첫 번째 포스트의 댓글들
            var firstPostMainReply = new PostReply
            {
                PostId = posts[0].PostId,
                Post = posts[0],
                UserId = users[1].UserId,
                User = users[1],
                Reply = "좋은 포스트네요!"
            };
            replies.Add(firstPostMainReply);

            // 첫 번째 포스트의 대댓글들
            replies.Add(new PostReply
            {
                PostId = posts[0].PostId,
                Post = posts[0],
                UserId = users[2].UserId,
                User = users[2],
                Reply = "네, 정말 좋은 포스트입니다!",
                ParentId = firstPostMainReply.PostReplyId,
                Parent = firstPostMainReply
            });

            replies.Add(new PostReply
            {
                PostId = posts[0].PostId,
                Post = posts[0],
                UserId = users[3].UserId,
                User = users[3],
                Reply = "저도 동의합니다!",
                ParentId = firstPostMainReply.PostReplyId,
                Parent = firstPostMainReply
            });

            // 두 번째 포스트의 댓글들
            var secondPostMainReply = new PostReply
            {
                PostId = posts[1].PostId,
                Post = posts[1],
                UserId = users[0].UserId,
                User = users[0],
                Reply = "네, 잘 지내고 있습니다!"
            };
            replies.Add(secondPostMainReply);

            // 두 번째 포스트의 대댓글들
            var secondPostNestedReply = new PostReply
            {
                PostId = posts[1].PostId,
                Post = posts[1],
                UserId = users[2].UserId,
                User = users[2],
                Reply = "저도 잘 지내고 있어요!",
                ParentId = secondPostMainReply.PostReplyId,
                Parent = secondPostMainReply
            };
            replies.Add(secondPostNestedReply);

            // 두 번째 포스트의 대대댓글
            replies.Add(new PostReply
            {
                PostId = posts[1].PostId,
                Post = posts[1],
                UserId = users[4].UserId,
                User = users[4],
                Reply = "좋은 소식이네요!",
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