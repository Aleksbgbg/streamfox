namespace Streamfox.Server.Persistence
{
    using System.IO;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using Streamfox.Server.Processing;
    using Streamfox.Server.VideoManagement;

    public class FromDiskMetadataRetriever : IMetadataRetriever
    {
        private readonly DirectoryHandler _metadataHandler;

        public FromDiskMetadataRetriever(DirectoryHandler metadataHandler)
        {
            _metadataHandler = metadataHandler;
        }

        public async Task<VideoMetadata> RetrieveMetadata(VideoId videoId)
        {
            using StreamReader streamReader =
                    new StreamReader(_metadataHandler.OpenRead(videoId.ToString()));
            return JsonConvert.DeserializeObject<VideoMetadata>(
                    await streamReader.ReadToEndAsync());
        }
    }
}