namespace Streamfox.Server.Persistence
{
    using System.IO;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using Streamfox.Server.Processing;
    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    public class ToDiskMetadataSaver : IMetadataSaver
    {
        private readonly DirectoryHandler _metadataHandler;

        public ToDiskMetadataSaver(DirectoryHandler metadataHandler)
        {
            _metadataHandler = metadataHandler;
        }

        public async Task SaveMetadata(VideoId videoId, VideoMetadata videoMetadata)
        {
            await using StreamWriter streamWriter =
                    new StreamWriter(_metadataHandler.OpenWrite(videoId.ToString()));
            await streamWriter.WriteAsync(JsonConvert.SerializeObject(videoMetadata));
        }
    }
}