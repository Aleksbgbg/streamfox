namespace Streamfox.Server.Tests.Unit.VideoProcessing
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Moq;

    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    using Xunit;

    public class VideoProcessorTest
    {
        private readonly VideoProcessor _videoProcessor;

        private readonly FakeFileSystem _fakeFileSystem;

        private readonly Mock<IVideoVerifier> _videoVerifier;

        private readonly Mock<IBackgroundVideoProcessor> _backgroundVideoProcessor;

        private readonly Mock<ITaskRunner> _taskRunner;

        private readonly Mock<IFramesFetcher> _framesFetcher;

        private readonly Mock<IVideoProgressStore> _videoProgressStore;

        private readonly Mock<IThumbnailExtractor> _thumbnailExtractor;

        public VideoProcessorTest()
        {
            _fakeFileSystem = new FakeFileSystem();
            _videoVerifier = new Mock<IVideoVerifier>();
            _backgroundVideoProcessor = new Mock<IBackgroundVideoProcessor>();
            _taskRunner = new Mock<ITaskRunner>();
            _framesFetcher = new Mock<IFramesFetcher>();
            _videoProgressStore = new Mock<IVideoProgressStore>();
            _thumbnailExtractor = new Mock<IThumbnailExtractor>();
            _thumbnailExtractor = new Mock<IThumbnailExtractor>();
            _videoProcessor = new VideoProcessor(
                    _fakeFileSystem,
                    _videoVerifier.Object,
                    _backgroundVideoProcessor.Object,
                    _taskRunner.Object,
                    _framesFetcher.Object,
                    _videoProgressStore.Object,
                    _thumbnailExtractor.Object);
        }

        [Fact]
        public async Task SavesVideoStream()
        {
            VideoId videoId = new VideoId(100);
            Stream videoStream = TestUtils.MockStream();

            await _videoProcessor.ProcessVideo(videoId, videoStream);

            Assert.Same(videoStream, _fakeFileSystem.VideoStream);
        }

        [Fact]
        public async Task InvalidVideo_ReturnsFalse()
        {
            VideoId videoId = new VideoId(200);
            Stream videoStream = TestUtils.MockStream();
            SetupInvalidVideo(videoId);

            bool processingSuccessful = await _videoProcessor.ProcessVideo(videoId, videoStream);

            Assert.False(processingSuccessful);
        }

        [Fact]
        public async Task ValidVideo_ReturnsTrue()
        {
            VideoId videoId = new VideoId(300);
            Stream videoStream = TestUtils.MockStream();
            SetupValidVideo(videoId);

            bool processingSuccessful = await _videoProcessor.ProcessVideo(videoId, videoStream);

            Assert.True(processingSuccessful);
        }

        [Fact]
        public async Task InvalidVideo_DeletesVideo()
        {
            VideoId videoId = new VideoId(400);
            Stream videoStream = TestUtils.MockStream();
            SetupInvalidVideo(videoId);

            await _videoProcessor.ProcessVideo(videoId, videoStream);

            Assert.True(_fakeFileSystem.DeletedVideo);
        }

        [Fact]
        public async Task ValidVideo_DoesNotDeleteVideo()
        {
            VideoId videoId = new VideoId(500);
            Stream videoStream = TestUtils.MockStream();
            SetupValidVideo(videoId);

            await _videoProcessor.ProcessVideo(videoId, videoStream);

            Assert.False(_fakeFileSystem.DeletedVideo);
        }

        [Fact]
        public async Task ValidVideo_BeginsProcessing()
        {
            VideoId videoId = new VideoId(600);
            Stream videoStream = TestUtils.MockStream();
            SetupValidVideo(videoId);
            Task backgroundProcessingTask = Task.FromResult(0xA);
            _backgroundVideoProcessor.Setup(processor => processor.ProcessVideo(videoId))
                                     .Returns(backgroundProcessingTask);

            await _videoProcessor.ProcessVideo(videoId, videoStream);

            _taskRunner.Verify(runner => runner.RunBackground(backgroundProcessingTask));
        }

        [Fact]
        public async Task StoresVideoInProgressStore()
        {
            VideoId videoId = new VideoId(700);
            const int frames = 575;
            SetupValidVideo(videoId);
            _framesFetcher.Setup(fetcher => fetcher.FetchVideoFrames(videoId)).ReturnsAsync(frames);

            await _videoProcessor.ProcessVideo(videoId, TestUtils.MockStream());

            _videoProgressStore.Verify(store => store.RegisterVideo(videoId, frames));
        }

        [Fact]
        public async Task ExtractsThumbnail()
        {
            VideoId videoId = new VideoId(800);
            SetupValidVideo(videoId);

            await _videoProcessor.ProcessVideo(videoId, TestUtils.MockStream());

            _thumbnailExtractor.Verify(extractor => extractor.ExtractThumbnail(videoId));
        }

        private void SetupValidVideo(VideoId videoId)
        {
            _videoVerifier.Setup(verifier => verifier.IsValidVideo(videoId))
                          .Returns(Task.FromResult(true));
        }

        private void SetupInvalidVideo(VideoId videoId)
        {
            _videoVerifier.Setup(verifier => verifier.IsValidVideo(videoId))
                          .Returns(Task.FromResult(false));
        }

        private class FakeFileSystem : IIntermediateVideoWriter
        {
            private VideoId _savedVideoId;

            public Stream VideoStream { get; private set; }

            public bool DeletedVideo { get; private set; }

            public Task SaveVideo(VideoId videoId, Stream videoStream)
            {
                _savedVideoId = videoId;
                VideoStream = videoStream;
                return Task.CompletedTask;
            }

            public void DeleteVideo(VideoId videoId)
            {
                if (videoId == _savedVideoId)
                {
                    _savedVideoId = new VideoId(0);
                    DeletedVideo = true;
                }
                else
                {
                    throw VideoNotSaved();
                }
            }

            private static InvalidOperationException VideoNotSaved()
            {
                return new InvalidOperationException(
                        "FakeFileSystem: EITHER video not saved OR video deleted too early");
            }
        }
    }
}