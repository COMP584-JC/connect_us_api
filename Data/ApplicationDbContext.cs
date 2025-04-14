using Microsoft.EntityFrameworkCore;
using connect_us_api.Models;

namespace connect_us_api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostReply> PostReplies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // 필요한 경우, cascade delete, 관계 정의를 더 명시할 수 있습니다.
            // modelBuilder.Entity<Post>()
            //    .HasOne(p => p.User)
            //    .WithMany()
            //    .HasForeignKey(p => p.UserId)
            //    .OnDelete(DeleteBehavior.Cascade);

            // 등등...
        }
    }
}
