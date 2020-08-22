namespace Streamfox.Server.Processing
{
    using System.Threading.Tasks;

    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    public class MultimediaFramework : IMultimediaFramework
    {
        private readonly IPathResolver _pathResolver;

        private readonly IFfmpeg _ffmpeg;

        public MultimediaFramework(IPathResolver pathResolver, IFfmpeg ffmpeg)
        {
            _pathResolver = pathResolver;
            _ffmpeg = ffmpeg;
        }

        public async Task ExtractVideoThumbnail(VideoId videoId)
        {
            string sourceVideoPath = _pathResolver.ResolveIntermediateVideoPath(videoId);
            string thumbnailPath = _pathResolver.ResolveThumbnailPath(videoId);

            await _ffmpeg.ExtractThumbnail(sourceVideoPath, thumbnailPath);
        }

        public async Task<VideoMetadata> ExtractVideoAndCoerceToSupportedFormats(VideoId videoId)
        {
            string sourcePath = _pathResolver.ResolveIntermediateVideoPath(videoId);
            string outputPath = _pathResolver.ResolveVideoPath(videoId);

            VideoMetadata videoMetadata = await _ffmpeg.GrabVideoMetadata(sourcePath);

            if ((videoMetadata.VideoCodec == VideoCodec.Vp9 &&
                 videoMetadata.VideoFormat == VideoFormat.Webm) ||
                (videoMetadata.VideoCodec == VideoCodec.H264 &&
                 videoMetadata.VideoFormat == VideoFormat.Mp4) ||
                (videoMetadata.VideoCodec == VideoCodec.H264 &&
                 videoMetadata.VideoFormat == VideoFormat.Webm))
            {
                await _ffmpeg.NoOpCopy(sourcePath, outputPath);
            }
            else if (videoMetadata.VideoCodec == VideoCodec.H264 &&
                     videoMetadata.VideoFormat == VideoFormat.Other)
            {
                await _ffmpeg.ConvertToMp4(sourcePath, outputPath);
                videoMetadata = new VideoMetadata(VideoCodec.H264, VideoFormat.Mp4);
            }
            else if (videoMetadata.VideoCodec != VideoCodec.Invalid)
            {
                await _ffmpeg.ConvertToVp9Webm(sourcePath, outputPath);
                videoMetadata = new VideoMetadata(VideoCodec.Vp9, VideoFormat.Webm);
            }

            return videoMetadata;
        }
    }
}