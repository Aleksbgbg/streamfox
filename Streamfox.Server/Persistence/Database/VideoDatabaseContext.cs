namespace Streamfox.Server.Persistence.Database
{
    using Microsoft.EntityFrameworkCore;

    public class VideoDatabaseContext : DbContext
    {
        public VideoDatabaseContext(DbContextOptions<VideoDatabaseContext> options) : base(options)
        {
        }

        public virtual DbSet<Video> Videos { get; protected set; }

        public virtual DbSet<Tag> Tags { get; protected set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<VideoTag>()
                   .HasKey(
                           videoTag => new
                           {
                               videoTag.VideoId,
                               videoTag.TagId
                           });
        }
    }
}