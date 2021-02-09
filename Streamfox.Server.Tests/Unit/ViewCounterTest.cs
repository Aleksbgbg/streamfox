namespace Streamfox.Server.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Server.IIS.Core;
    using Microsoft.Extensions.DependencyInjection;
    using Streamfox.Server.Controllers;
    using Streamfox.Server.Persistence;
    using Streamfox.Server.VideoManagement;
    using Xunit;

    public class ViewCounterTest
    {
        private const float RequiredWatchFraction = 0.8f;
        
        private readonly ClockStub _currentTime;

        private readonly VideoViewQueriesFake _videoDatabase;

        public ViewCounterTest()
        {
            _currentTime = new ClockStub(new DateTime(2020, 1, 1));
            _videoDatabase = new VideoViewQueriesFake();
        }

        [Fact]
        public void ValidView_Counted()
        {
            VideoId videoId = new VideoId(123);
            TimeSpan videoLength = TimeSpan.FromSeconds(10);
            _videoDatabase.SetVideoLength(videoId, videoLength);
            TimeSpan minimumRequiredWatchTime = videoLength.Multiply(RequiredWatchFraction);

            ViewId viewId = ViewController().BeginWatching(videoId);
            _currentTime.AdvanceBy(minimumRequiredWatchTime);
            ViewController().StillWatching(viewId);

            Assert.Equal(1, ViewController().GetViews(videoId));
        }

        [Fact]
        public void InvalidView_NotCounted()
        {
            VideoId videoId = new VideoId(123);
            TimeSpan videoLength = TimeSpan.FromHours(10);
            _videoDatabase.SetVideoLength(videoId, videoLength);
            TimeSpan watchTime = videoLength.Multiply(RequiredWatchFraction).Subtract(TimeSpan.FromSeconds(1));

            ViewId viewId = ViewController().BeginWatching(videoId);
            _currentTime.AdvanceBy(watchTime);
            ViewController().StillWatching(viewId);

            Assert.Equal(0, ViewController().GetViews(videoId));
        }

        [Fact]
        public void SeparateVideos_HaveSeparateViews()
        {
            VideoId videoId1 = new VideoId(123);
            VideoId videoId2 = new VideoId(456);
            _videoDatabase.SetVideoLength(videoId1, TimeSpan.Zero);
            _videoDatabase.SetVideoLength(videoId2, TimeSpan.Zero);

            ViewId viewVideo1 = ViewController().BeginWatching(videoId1);
            ViewId viewVideo2 = ViewController().BeginWatching(videoId2);
            ViewController().StillWatching(viewVideo1);
            ViewController().StillWatching(viewVideo2);

            Assert.Equal(1, ViewController().GetViews(videoId1));
            Assert.Equal(1, ViewController().GetViews(videoId2));
        }

        [Fact]
        public void SeparateUsers_HaveSeparateViews()
        {
            VideoId videoId = new VideoId(123);
            _videoDatabase.SetVideoLength(videoId, TimeSpan.FromMinutes(15));

            // Counted
            ViewId user1 = ViewController().BeginWatching(videoId);
            _currentTime.AdvanceBy(TimeSpan.FromMinutes(15));
            ViewController().StillWatching(user1);
            // Not counted
            ViewId user2 = ViewController().BeginWatching(videoId);
            ViewController().StillWatching(user2);

            Assert.Equal(1, ViewController().GetViews(videoId));
        }

        [Fact]
        public void DuplicateView_CountedOnce()
        {
            VideoId videoId = new VideoId(123);
            _videoDatabase.SetVideoLength(videoId, TimeSpan.Zero);

            ViewId viewId = ViewController().BeginWatching(videoId);
            ViewController().StillWatching(viewId);
            ViewController().StillWatching(viewId);

            Assert.Equal(1, ViewController().GetViews(videoId));
        }

        private ViewController ViewController()
        {
            return new ViewController(_currentTime, _videoDatabase, new SnowflakeIdGenerator());
        }

        private class ClockStub : IClock
        {
            private DateTime _currentTime;

            public ClockStub(DateTime currentTime)
            {
                _currentTime = currentTime;
            }

            public void AdvanceBy(TimeSpan time)
            {
                _currentTime = _currentTime.Add(time);
            }

            public DateTime CurrentTime()
            {
                return _currentTime;
            }
        }
        
        private class VideoViewQueriesFake : IVideoViewQueries
        {
            private readonly Dictionary<VideoId, int> _views = new Dictionary<VideoId, int>();

            private readonly Dictionary<VideoId, TimeSpan> _videoLengths = new Dictionary<VideoId, TimeSpan>();
            
            public void AddView(VideoId videoId)
            {
                if (!_views.ContainsKey(videoId))
                {
                    _views.Add(videoId, 0);
                }
                
                _views[videoId] += 1;
            }

            public int GetViews(VideoId videoId)
            {
                if (!_views.ContainsKey(videoId))
                {
                    return 0;
                }
                
                return _views[videoId];
            }

            public void SetVideoLength(VideoId videoId, TimeSpan videoLength)
            {
                _videoLengths.Add(videoId, videoLength);
            }

            public TimeSpan VideoLength(VideoId videoId)
            {
                if (_videoLengths.ContainsKey(videoId))
                {
                    return _videoLengths[videoId];
                }

                throw new ArgumentException("VideoId does not exist");
            }
        }
    }
}