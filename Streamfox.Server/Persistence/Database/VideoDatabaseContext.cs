namespace Streamfox.Server.Persistence.Database
{
    using System.Linq;

    using Microsoft.EntityFrameworkCore;

    using Streamfox.Server.VideoManagement;

    public class VideoDatabaseContext : DbContext, IVideoExistenceChecker, IVideoLister
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

        public bool VideoExists(VideoId videoId)
        {
            return Videos.Any(video => video.Id == videoId.Value);
        }

        public VideoId[] ListLabels()
        {
            return Videos.OrderBy(video => video.Id)
                         .Select(video => new VideoId(video.Id))
                         .ToArray();
        }
    }
}