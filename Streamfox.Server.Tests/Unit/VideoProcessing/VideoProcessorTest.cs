namespace Streamfox.Server.Tests.Unit.VideoProcessing
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using Moq;

    using Streamfox.Server.Processing;
    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    using Xunit;

    public class VideoProcessorTest
    {
        private readonly VideoProcessor _videoProcessor;

        private readonly Mock<IVideoOperationRunner> _videoOperationRunner;

        private readonly FakeFileSystem _fakeFileSystem;

        private readonly Mock<IExistenceChecker> _existenceChecker;

        private readonly Mock<IMetadataSaver> _metadataSaver;

        public VideoProcessorTest()
        {
            _videoOperationRunner = new Mock<IVideoOperationRunner>();
            _fakeFileSystem = new FakeFileSystem();
            MultimediaProcessor multimediaProcessor = new MultimediaProcessor(
                    _fakeFileSystem,
                    _videoOperationRunner.Object);
            _existenceChecker = new Mock<IExistenceChecker>();
            _metadataSaver = new Mock<IMetadataSaver>();
            _videoProcessor = new VideoProcessor(
                    _fakeFileSystem,
                    multimediaProcessor,
                    _existenceChecker.Object,
                    _metadataSaver.Object);
        }

        public static IEnumerable<object[]> VideoCases => new[]
        {
            new object[] { new VideoId(100) },
            new object[] { new VideoId(200) }
        };

        [Theory]
        [MemberData(nameof(VideoCases))]
        public async Task ExtractsThumbnailFromIntermediateVideo(VideoId videoId)
        {
            Stream videoStream = TestUtils.MockStream();

            await _videoProcessor.ProcessVideo(videoId, videoStream);

            _videoOperationRunner.Verify(
                    ffmpeg => ffmpeg.ExtractThumbnail(
                            $"intermediate-{videoId}",
                            $"thumbnail-{videoId}"));
        }

        [Fact]
        public async Task SavesVideoStream()
        {
            Stream videoStream = TestUtils.MockStream();

            await _videoProcessor.ProcessVideo(new VideoId(100), videoStream);

            Assert.Same(videoStream, _fakeFileSystem.VideoStream);
        }

        [Theory]
        [MemberData(nameof(VideoCases))]
        public async Task DeletesIntermediateVideoAfterExtraction(VideoId videoId)
        {
            Stream videoStream = TestUtils.MockStream();

            await _videoProcessor.ProcessVideo(videoId, videoStream);

            Assert.True(_fakeFileSystem.DeletedVideo);
        }

        [Theory]
        [MemberData(nameof(VideoCases))]
        public async Task CopiesVideoToOutputWhenH264Mp4(VideoId videoId)
        {
            SetupVideoMetadata(videoId, VideoCodec.H264, VideoFormat.Mp4);
            Stream videoStream = TestUtils.MockStream();

            await _videoProcessor.ProcessVideo(videoId, videoStream);

            _videoOperationRunner.Verify(
                    ffmpeg => ffmpeg.NoOpCopy($"intermediate-{videoId}", $"video-{videoId}"));
        }

        [Theory]
        [MemberData(nameof(VideoCases))]
        public async Task CopiesVideoToOutputWhenVp9Webm(VideoId videoId)
        {
            SetupVideoMetadata(videoId, VideoCodec.Vp9, VideoFormat.Webm);
            Stream videoStream = TestUtils.MockStream();

            await _videoProcessor.ProcessVideo(videoId, videoStream);

            _videoOperationRunner.Verify(
                    ffmpeg => ffmpeg.NoOpCopy($"intermediate-{videoId}", $"video-{videoId}"));
        }

        [Theory]
        [MemberData(nameof(VideoCases))]
        public async Task DoesNotCopyVideoToOutputWhenInvalidCodec(VideoId videoId)
        {
            SetupVideoMetadata(videoId, VideoCodec.Invalid, VideoFormat.Other);
            Stream videoStream = TestUtils.MockStream();

            await _videoProcessor.ProcessVideo(videoId, videoStream);

            _videoOperationRunner.Verify(
                    ffmpeg => ffmpeg.NoOpCopy($"intermediate-{videoId}", $"video-{videoId}"),
                    Times.Never);
        }

        [Theory]
        [MemberData(nameof(VideoCases))]
        public async Task ConvertsVideoToVp9WhenOtherCodec(VideoId videoId)
        {
            SetupVideoMetadata(videoId, VideoCodec.Other, VideoFormat.Mp4);
            Stream videoStream = TestUtils.MockStream();

            await _videoProcessor.ProcessVideo(videoId, videoStream);

            _videoOperationRunner.Verify(
                    ffmpeg => ffmpeg.ConvertToVp9Webm(
                            $"intermediate-{videoId}",
                            $"video-{videoId}"));
        }

        [Theory]
        [MemberData(nameof(VideoCases))]
        public async Task ConvertsVideoToVp9WhenH264OtherFormat(VideoId videoId)
        {
            SetupVideoMetadata(videoId, VideoCodec.H264, VideoFormat.Other);
            Stream videoStream = TestUtils.MockStream();

            await _videoProcessor.ProcessVideo(videoId, videoStream);

            _videoOperationRunner.Verify(
                    ffmpeg => ffmpeg.ConvertToVp9Webm(
                            $"intermediate-{videoId}",
                            $"video-{videoId}"));
        }

        [Theory]
        [MemberData(nameof(VideoCases))]
        public async Task ConvertsVideoToVp9WhenH264Webm(VideoId videoId)
        {
            SetupVideoMetadata(videoId, VideoCodec.H264, VideoFormat.Webm);
            Stream videoStream = TestUtils.MockStream();

            await _videoProcessor.ProcessVideo(videoId, videoStream);

            _videoOperationRunner.Verify(
                    ffmpeg => ffmpeg.ConvertToVp9Webm(
                            $"intermediate-{videoId}",
                            $"video-{videoId}"));
        }

        [Theory]
        [MemberData(nameof(VideoCases))]
        public async Task SavesVideoMetadata_Mp4(VideoId videoId)
        {
            SetupVideoMetadata(videoId, VideoCodec.H264, VideoFormat.Mp4);
            Stream videoStream = TestUtils.MockStream();

            await _videoProcessor.ProcessVideo(videoId, videoStream);

            _metadataSaver.Verify(
                    saver => saver.SaveMetadata(
                            videoId,
                            new VideoMetadata(VideoCodec.H264, VideoFormat.Mp4)));
        }

        [Theory]
        [MemberData(nameof(VideoCases))]
        public async Task UploadFailed_DoesntSaveVideoMetadata(VideoId videoId)
        {
            _existenceChecker.Setup(checker => checker.ThumbnailExists(videoId)).Returns(false);
            SetupVideoMetadata(videoId, VideoCodec.H264, VideoFormat.Mp4);
            Stream videoStream = TestUtils.MockStream();

            await _videoProcessor.ProcessVideo(videoId, videoStream);

            _metadataSaver.Verify(
                    saver => saver.SaveMetadata(
                            videoId,
                            new VideoMetadata(VideoCodec.H264, VideoFormat.Mp4)),
                    Times.Never);
        }

        [Theory]
        [MemberData(nameof(VideoCases))]
        public async Task SavesVideoMetadata_Webm(VideoId videoId)
        {
            SetupVideoMetadata(videoId, VideoCodec.Vp9, VideoFormat.Webm);
            Stream videoStream = TestUtils.MockStream();

            await _videoProcessor.ProcessVideo(videoId, videoStream);

            _metadataSaver.Verify(
                    saver => saver.SaveMetadata(
                            videoId,
                            new VideoMetadata(VideoCodec.Vp9, VideoFormat.Webm)));
        }

        [Theory]
        [MemberData(nameof(VideoCases))]
        public async Task ReturnsSuccessWhenThumbnailAndVideoExist(VideoId videoId)
        {
            SetupThumbnailExists(videoId);
            SetupVideoExists(videoId);

            bool success = await _videoProcessor.ProcessVideo(videoId, TestUtils.MockStream());

            Assert.True(success);
        }

        [Theory]
        [MemberData(nameof(VideoCases))]
        public async Task ReturnsSuccessWhenThumbnailDoesNotExistButVideoDoes(VideoId videoId)
        {
            SetupThumbnailDoesNotExist(videoId);
            SetupVideoExists(videoId);

            bool success = await _videoProcessor.ProcessVideo(videoId, TestUtils.MockStream());

            Assert.False(success);
        }

        [Theory]
        [MemberData(nameof(VideoCases))]
        public async Task ReturnsFailureWhenThumbnailExistsButVideoDoesNot(VideoId videoId)
        {
            SetupThumbnailExists(videoId);
            SetupVideoDoesNotExist(videoId);

            bool success = await _videoProcessor.ProcessVideo(videoId, TestUtils.MockStream());

            Assert.False(success);
        }

        [Theory]
        [MemberData(nameof(VideoCases))]
        public async Task ReturnsFailureWhenThumbnailAndVideoDoNotExist(VideoId videoId)
        {
            SetupThumbnailDoesNotExist(videoId);
            SetupVideoDoesNotExist(videoId);

            bool success = await _videoProcessor.ProcessVideo(videoId, TestUtils.MockStream());

            Assert.False(success);
        }

        private void SetupVideoMetadata(
                VideoId videoId, VideoCodec videoCodec, VideoFormat videoFormat)
        {
            _videoOperationRunner
                    .Setup(ffmpeg => ffmpeg.GrabVideoMetadata($"intermediate-{videoId}"))
                    .Returns(Task.FromResult(new VideoMetadata(videoCodec, videoFormat)));
        }

        private void SetupThumbnailExists(VideoId videoId)
        {
            _existenceChecker.Setup(checker => checker.ThumbnailExists(videoId)).Returns(true);
        }

        private void SetupThumbnailDoesNotExist(VideoId videoId)
        {
            _existenceChecker.Setup(checker => checker.ThumbnailExists(videoId)).Returns(false);
        }

        private void SetupVideoExists(VideoId videoId)
        {
            _existenceChecker.Setup(checker => checker.VideoExists(videoId)).Returns(true);
        }

        private void SetupVideoDoesNotExist(VideoId videoId)
        {
            _existenceChecker.Setup(checker => checker.VideoExists(videoId)).Returns(false);
        }

        private class FakeFileSystem : IIntermediateVideoWriter, IPathResolver
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