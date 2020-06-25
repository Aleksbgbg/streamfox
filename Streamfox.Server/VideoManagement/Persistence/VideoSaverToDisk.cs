namespace Streamfox.Server.VideoManagement.Persistence
{
    using System.IO;
    using System.Threading.Tasks;

    public class VideoSaverToDisk : IVideoSaver
    {
        private readonly IVideoFileWriter _videoFileWriter;

        private readonly IThumbnailFileWriter _thumbnailFileWriter;

        public VideoSaverToDisk(IVideoFileWriter videoFileWriter, IThumbnailFileWriter thumbnailFileWriter)
        {
            _videoFileWriter = videoFileWriter;
            _thumbnailFileWriter = thumbnailFileWriter;
        }

        public async Task SaveVideo(string label, Stream video, Stream thumbnail)
        {
            await using (Stream videoFileStream = _videoFileWriter.OpenWrite(label))
            {
                await video.CopyToAsync(videoFileStream);
            }

            await using (Stream thumbnailFileStream = _thumbnailFileWriter.OpenWrite(label))
            {
                await thumbnail.CopyToAsync(thumbnailFileStream);
            }
        }
    }
}