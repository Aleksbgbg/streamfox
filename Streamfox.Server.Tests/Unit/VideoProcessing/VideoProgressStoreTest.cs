namespace Streamfox.Server.Tests.Unit.VideoProcessing
{
    using System;

    using Moq;

    using Streamfox.Server.Types;
    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    using Xunit;
    using Xunit.Sdk;

    public class VideoProgressStoreTest
    {
        private readonly VideoProgressStore _videoProgressStore;

        private readonly Mock<IClockFactory> _clockFactory;

        private readonly FakeClock _clock;

        public VideoProgressStoreTest()
        {
            _clockFactory = new Mock<IClockFactory>();
            _clock = new FakeClock();
            _clockFactory.Setup(factory => factory.CreateClock()).Returns(_clock);
            _videoProgressStore = new VideoProgressStore(_clockFactory.Object);
        }

        [Fact]
        public void RegisterVideo_HasProgress()
        {
            VideoId videoId = new VideoId(10);

            _videoProgressStore.RegisterVideo(videoId, 0);
            Optional<ConversionProgress> optionalConversionProgress =
                    _videoProgressStore.RetrieveProgress(videoId);

            Assert.True(
                    optionalConversionProgress.HasValue,
                    "Empty progress report after registering video");
        }

        [Fact]
        public void NoReports_ReturnsEmpty()
        {
            VideoId videoId = new VideoId(10);

            Optional<ConversionProgress> optionalConversionProgress =
                    _videoProgressStore.RetrieveProgress(videoId);

            Assert.False(
                    optionalConversionProgress.HasValue,
                    "Progress retrieved but video not registered");
        }

        [Fact]
        public void RegisterVideo_ReturnsDefaultProgress()
        {
            VideoId videoId = new VideoId(10);

            _videoProgressStore.RegisterVideo(videoId, 1000);
            ConversionProgress optionalConversionProgress =
                    _videoProgressStore.RetrieveProgress(videoId).Value;

            Assert.Equal(0, optionalConversionProgress.CurrentFrame);
            Assert.Equal(1000, optionalConversionProgress.VideoDuration);
            Assert.Equal(TimeSpan.Zero, optionalConversionProgress.TimeElapsed);
            Assert.Equal(TimeSpan.Zero, optionalConversionProgress.TimeRemaining);
        }

        [Fact]
        public void ReportProgress_ReturnsLatestFrame()
        {
            VideoId videoId = new VideoId(10);

            _videoProgressStore.RegisterVideo(videoId, 1000);
            _videoProgressStore.ReportProgress(videoId, 99);
            _videoProgressStore.ReportProgress(videoId, 250);
            ConversionProgress optionalConversionProgress =
                    _videoProgressStore.RetrieveProgress(videoId).Value;

            Assert.Equal(250, optionalConversionProgress.CurrentFrame);
        }

        [Fact]
        public void ReportProgress_AfterRegister_NoTimeElapsed()
        {
            VideoId videoId = new VideoId(10);

            _videoProgressStore.RegisterVideo(videoId, 1000);
            ConversionProgress optionalConversionProgress =
                    _videoProgressStore.RetrieveProgress(videoId).Value;

            Assert.Equal(TimeSpan.Zero, optionalConversionProgress.TimeElapsed);
        }

        [Fact]
        public void ReportProgress_ReportsTimeElapsedCorrectly()
        {
            VideoId videoId = new VideoId(10);

            _videoProgressStore.RegisterVideo(videoId, 1000);
            _clock.FastForward(TimeSpan.FromSeconds(5));
            _videoProgressStore.ReportProgress(videoId, 250);
            _clock.FastForward(TimeSpan.FromSeconds(10));
            ConversionProgress optionalConversionProgress =
                    _videoProgressStore.RetrieveProgress(videoId).Value;

            Assert.Equal(TimeSpan.FromSeconds(10), optionalConversionProgress.TimeElapsed);
        }

        [Theory]
        [InlineData(25, 10, 30)]
        [InlineData(50, 5, 5)]
        [InlineData(1, 10, 990)]
        public void ReportProgress_ReportsTimeRemainingCorrectly(
                int donePercent, int elapsedTime, int remainingTime)
        {
            VideoId videoId = new VideoId(10);

            _videoProgressStore.RegisterVideo(videoId, 100);
            _videoProgressStore.ReportProgress(videoId, donePercent);
            _clock.FastForward(TimeSpan.FromSeconds(elapsedTime));
            ConversionProgress optionalConversionProgress =
                    _videoProgressStore.RetrieveProgress(videoId).Value;

            Assert.Equal(
                    TimeSpan.FromSeconds(remainingTime),
                    optionalConversionProgress.TimeRemaining);
        }

        [Fact]
        public void RetrieveProgress_MultipleEntries_ReturnsCorrectFrame()
        {
            VideoId videoId1 = new VideoId(10);
            const int frame1 = 50;
            VideoId videoId2 = new VideoId(20);
            const int frame2 = 99;

            _videoProgressStore.RegisterVideo(videoId1, frame1 + 1);
            _videoProgressStore.RegisterVideo(videoId2, frame2 + 1);
            _videoProgressStore.ReportProgress(videoId1, frame1);
            _videoProgressStore.ReportProgress(videoId2, frame2);
            Optional<ConversionProgress> optionalConversionProgress1 =
                    _videoProgressStore.RetrieveProgress(videoId1);
            Optional<ConversionProgress> optionalConversionProgress2 =
                    _videoProgressStore.RetrieveProgress(videoId2);

            Assert.Equal(frame1, optionalConversionProgress1.Value.CurrentFrame);
            Assert.Equal(frame2, optionalConversionProgress2.Value.CurrentFrame);
        }

        [Theory]
        [InlineData(60, 60)]
        [InlineData(120, 140)]
        [InlineData(61, int.MaxValue)]
        public void ReportCompleted_StopsStoring(int totalFrames, int lastReportedFrame)
        {
            VideoId videoId = new VideoId(10);

            _videoProgressStore.RegisterVideo(videoId, totalFrames);
            _videoProgressStore.ReportProgress(videoId, lastReportedFrame);
            Optional<ConversionProgress> optionalConversionProgress =
                    _videoProgressStore.RetrieveProgress(videoId);

            Assert.False(optionalConversionProgress.HasValue);
        }

        [Fact]
        // TODO: This case should not occur - see if FFMPEG will always report the last frame
        public void ReportAfterCompletion_IgnoresReport()
        {
            VideoId videoId = new VideoId(10);

            _videoProgressStore.RegisterVideo(videoId, 60);
            _videoProgressStore.ReportProgress(videoId, 60);
            _videoProgressStore.ReportProgress(videoId, int.MaxValue);
            Optional<ConversionProgress> optionalConversionProgress =
                    _videoProgressStore.RetrieveProgress(videoId);

            Assert.False(optionalConversionProgress.HasValue);
        }

        private class FakeClock : IClock
        {
            private bool _started;

            private TimeSpan _elapsedTime = TimeSpan.Zero;

            public void FastForward(TimeSpan timeSpan)
            {
                if (_started)
                {
                    _elapsedTime += timeSpan;
                }
            }

            public void VerifyStarted()
            {
                if (!_started)
                {
                    throw new XunitException("Clock never started");
                }
            }

            public bool IsStarted()
            {
                return _started;
            }

            public void Start()
            {
                if (_started)
                {
                    throw new XunitException("Clock started multiple times");
                }

                _started = true;
            }

            public TimeSpan ElapsedTime()
            {
                return _elapsedTime;
            }
        }
    }
}