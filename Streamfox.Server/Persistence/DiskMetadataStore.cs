namespace Streamfox.Server.Persistence
{
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using Streamfox.Server.Persistence.Operations;
    using Streamfox.Server.Processing;
    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    public class DiskMetadataStore : IMetadataSaver, IMetadataRetriever
    {
        private readonly IFileWriter _fileWriter;

        private readonly IFileReader _fileReader;

        public DiskMetadataStore(IFileWriter fileWriter, IFileReader fileReader)
        {
            _fileWriter = fileWriter;
            _fileReader = fileReader;
        }

        public async Task SaveMetadata(VideoId videoId, VideoMetadata videoMetadata)
        {
            await _fileWriter.Write(videoId.ToString(), JsonConvert.SerializeObject(videoMetadata));
        }

        public async Task<VideoMetadata> RetrieveMetadata(VideoId videoId)
        {
            return JsonConvert.DeserializeObject<VideoMetadata>(
                    await _fileReader.Read(videoId.ToString()));
        }
    }
}