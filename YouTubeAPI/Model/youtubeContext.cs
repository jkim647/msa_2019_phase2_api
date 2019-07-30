using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace YouTubeAPI.Model
{
    public partial class youtubeContext : DbContext
    {
        public youtubeContext()
        {
        }

        public youtubeContext(DbContextOptions<youtubeContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Comments> Comments { get; set; }
        public virtual DbSet<Transcription> Transcription { get; set; }
        public virtual DbSet<Video> Video { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=tcp:jungsyoutube.database.windows.net,1433;Initial Catalog=youtube;Persist Security Info=False;User ID=jungsup;Password=Kimyc072;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<Comments>(entity =>
            {
                entity.Property(e => e.Comments1).IsUnicode(false);

                entity.Property(e => e.UserId).IsUnicode(false);

                entity.HasOne(d => d.VideoKeyNavigation)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.VideoKey)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("VideoKey");
            });

            modelBuilder.Entity<Transcription>(entity =>
            {
                entity.Property(e => e.Phrase).IsUnicode(false);

                entity.HasOne(d => d.Video)
                    .WithMany(p => p.Transcription)
                    .HasForeignKey(d => d.VideoId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("VideoId");
            });

            modelBuilder.Entity<Video>(entity =>
            {
                entity.Property(e => e.ThumbnailUrl).IsUnicode(false);

                entity.Property(e => e.VideoTitle).IsUnicode(false);

                entity.Property(e => e.WebUrl).IsUnicode(false);
            });
        }
    }
}
