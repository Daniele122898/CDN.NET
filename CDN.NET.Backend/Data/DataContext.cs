using CDN.NET.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace CDN.NET.Backend.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        { }

        //public DbSet<UFile> UFiles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<UFile> UFiles { get; set; }
        public DbSet<Album> Albums { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ApiKey>()
                .HasOne(u => u.User)
                .WithOne(x => x.ApiKey)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UFile>()
                .HasOne(u => u.Owner)
                .WithMany(f => f.Files)
                .HasForeignKey(k => k.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UFile>()
                .HasOne(x => x.Album)
                .WithMany(x => x.UFiles)
                .HasForeignKey(k => k.AlbumId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            builder.Entity<Album>()
                .HasMany(a => a.UFiles)
                .WithOne(f => f.Album)
                .HasForeignKey(k => k.AlbumId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Album>()
                .HasOne(u => u.Owner)
                .WithMany(a => a.Albums)
                .HasForeignKey(k => k.OwnerId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            /*
            // Like
            builder.Entity<Like>()
                .HasKey(k => new {k.LikerId, k.LikeeId});

            builder.Entity<Like>()
                .HasOne(u => u.Likee)
                .WithMany(u => u.Likers)
                .HasForeignKey(u => u.LikeeId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<Like>()
                .HasOne(u => u.Liker)
                .WithMany(u => u.Likees)
                .HasForeignKey(u => u.LikerId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Messages
            builder.Entity<Message>()
                .HasOne(u => u.Sender)
                .WithMany(m => m.MessagesSent)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<Message>()
                .HasOne(u => u.Recipient)
                .WithMany(m => m.MessagesReceived)
                .OnDelete(DeleteBehavior.Restrict);
            */
        }
    }
}