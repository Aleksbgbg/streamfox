namespace Streamfox.Server.Processing
{
    using System.Threading.Tasks;

    using Streamfox.Server.Persistence;
    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    public class FramesFetcher : IFramesFetcher
    {
        private readonly IVideoFramesFetcher _videoFramesFetcher;

        private readonly IIntermediateVideoPathResolver _intermediateVideoPathResolver;

        public FramesFetcher(
                IVideoFramesFetcher videoFramesFetcher,
                IIntermediateVideoPathResolver intermediateVideoPathResolver)
        {
            _videoFramesFetcher = videoFramesFetcher;
            _intermediateVideoPathResolver = intermediateVideoPathResolver;
        }

        public Task<int> FetchVideoFrames(VideoId videoId)
        {
            return _videoFramesFetcher.FetchVideoFrames(
                    _intermediateVideoPathResolver.ResolveIntermediateVideoPath(videoId));
        }
    }
}