namespace Streamfox.Server.Processing
{
    public struct VideoMetadata
    {
        public VideoMetadata(VideoCodec videoCodec, VideoFormat videoFormat)
        {
            VideoCodec = videoCodec;
            VideoFormat = videoFormat;
        }

        public VideoCodec VideoCodec { get; }

        public VideoFormat VideoFormat { get; }
    }
}