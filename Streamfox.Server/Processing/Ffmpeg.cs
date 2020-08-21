namespace Streamfox.Server.Processing
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    public class Ffmpeg : IFfmpeg
    {
        private readonly IProcessRunner _processRunner;

        public Ffmpeg(IProcessRunner processRunner)
        {
            _processRunner = processRunner;
        }

        public async Task ExtractThumbnail(string videoPath, string thumbnailPath)
        {
            await _processRunner.RunFfmpeg(
                    $"-i \"{videoPath}\" -vframes 1 -q:v 5 -vf scale=-1:225 -f singlejpeg \"{thumbnailPath}\"");
        }

        public async Task<VideoMetadata> GrabVideoMetadata(string videoPath)
        {
            string result = await _processRunner.RunFfprobe(
                    $"-v quiet -show_streams -show_format -print_format json \"{videoPath}\"");

            FfmpegVideoMetadata ffmpegVideoMetadata =
                    JsonConvert.DeserializeObject<FfmpegVideoMetadata>(result);

            if (ffmpegVideoMetadata == null)
            {
                return new VideoMetadata(VideoCodec.Invalid, VideoFormat.Other);
            }

            VideoCodec videoCodec;
            VideoFormat videoFormat;

            Format container = ffmpegVideoMetadata.Format;

            if (ffmpegVideoMetadata.Streams == null ||
                ffmpegVideoMetadata.Streams.Length == 0 ||
                ffmpegVideoMetadata.Streams.All(stream => stream.CodecType != "video"))
            {
                videoCodec = VideoCodec.Invalid;
            }
            else
            {
                AudioVideoStream stream =
                        ffmpegVideoMetadata.Streams.First(stream => stream.CodecType == "video");

                if (stream.CodecName == "h264")
                {
                    videoCodec = VideoCodec.H264;
                }
                else if (stream.CodecName == "vp9")
                {
                    videoCodec = VideoCodec.Vp9;
                }
                else if (double.Parse(container.Duration) > 1d)
                {
                    videoCodec = VideoCodec.Other;
                }
                else
                {
                    videoCodec = VideoCodec.Invalid;
                }
            }

            if (container == null)
            {
                videoFormat = VideoFormat.Other;
            }
            else if (container.FormatName.Contains("mp4"))
            {
                videoFormat = VideoFormat.Mp4;
            }
            else if (container.FormatName.Contains("webm"))
            {
                videoFormat = VideoFormat.Webm;
            }
            else
            {
                videoFormat = VideoFormat.Other;
            }

            return new VideoMetadata(videoCodec, videoFormat);
        }

        public async Task ConvertToVp9Webm(string sourcePath, string outputPath)
        {
            await _processRunner.RunFfmpeg(
                    $"-i \"{sourcePath}\" -c:v vp9 -f webm \"{outputPath}\"");
        }

        public async Task ConvertToMp4(string sourcePath, string outputPath)
        {
            await _processRunner.RunFfmpeg(
                    $"-i \"{sourcePath}\" -c:v copy -f mp4 \"{outputPath}\"");
        }

        public Task NoOpCopy(string sourcePath, string outputPath)
        {
            File.Copy(sourcePath, outputPath);
            return Task.CompletedTask;
        }
    }
}