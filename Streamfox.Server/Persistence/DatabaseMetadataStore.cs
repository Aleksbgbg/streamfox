namespace Streamfox.Server.Persistence
{
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Extensions.DependencyInjection;

    using Streamfox.Server.Persistence.Database;
    using Streamfox.Server.Processing;
    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    public class DatabaseMetadataStore : IMetadataSaver, IMetadataRetriever
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public DatabaseMetadataStore(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task SaveMetadata(VideoId videoId, VideoMetadata videoMetadata)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            VideoDatabaseContext videoDatabaseContext = scope.ServiceProvider.GetService<VideoDatabaseContext>();

            await videoDatabaseContext.Videos.AddAsync(
                    new Video
                    {
                        Id = videoId.Value,
                        Name = null,
                        Views = 0,
                        Codec = videoMetadata.VideoCodec,
                        Format = videoMetadata.VideoFormat
                    });
            await videoDatabaseContext.SaveChangesAsync();
        }

        public Task<VideoMetadata> RetrieveMetadata(VideoId videoId)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            VideoDatabaseContext videoDatabaseContext =
                    scope.ServiceProvider.GetService<VideoDatabaseContext>();

            Video video = videoDatabaseContext.Videos.Single(video => video.Id == videoId.Value);
            return Task.FromResult(new VideoMetadata(video.Codec, video.Format));
        }
    }
}