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

        public VideoProcessorTest()
        {
            _fakeFileSystem = new FakeFileSystem();
            _videoVerifier = new Mock<IVideoVerifier>();
            _backgroundVideoProcessor = new Mock<IBackgroundVideoProcessor>();
            _taskRunner = new Mock<ITaskRunner>();
            _videoProcessor = new VideoProcessor(
                    _fakeFileSystem,
                    _videoVerifier.Object,
                    _backgroundVideoProcessor.Object,
                    _taskRunner.Object);
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

        private class FakeFileSystem : IIntermediateVideoWriter, IVideoComponentPathResolver
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
                    _savedVideoId = new VideoId();
                    DeletedVideo = true;
                }
                else
                {
                    throw VideoNotSaved();
                }
            }

            public string ResolveIntermediateVideoPath(VideoId videoId)
            {
                if (videoId == _savedVideoId)
                {
                    return $"intermediate-{videoId}";
                }

                throw VideoNotSaved();
            }

            public string ResolveThumbnailPath(VideoId videoId)
            {
                if (videoId == _savedVideoId)
                {
                    return $"thumbnail-{videoId}";
                }

                throw VideoNotSaved();
            }

            public string ResolveVideoPath(VideoId videoId)
            {
                if (videoId == _savedVideoId)
                {
                    return $"video-{videoId}";
                }

                throw VideoNotSaved();
            }

            private static InvalidOperationException VideoNotSaved()
            {
                return new InvalidOperationException(
                        "FakeFileSystem: EITHER video not saved OR video deleted too early");
            }
        }
    }
}