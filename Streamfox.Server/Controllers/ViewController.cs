namespace Streamfox.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using Streamfox.Server.VideoManagement;

    [ApiController]
    [Route("api/views")]
    public class ViewController
    {
        private readonly IClock _clock;
        
        private readonly IVideoViewQueries _videoViewQueries;
        
        private readonly IViewIdGenerator _viewIdGenerator;

        private static readonly Dictionary<ViewId, ViewMetadata> ViewMetadatas = new Dictionary<ViewId, ViewMetadata>();

        public ViewController(IClock clock, IVideoViewQueries videoViewQueries, IViewIdGenerator viewIdGenerator)
        {
            _clock = clock;
            _videoViewQueries = videoViewQueries;
            _viewIdGenerator = viewIdGenerator;
        }
        
        public int GetViews(VideoId videoId)
        {
            return _videoViewQueries.GetViews(videoId);
        }

        public ViewId BeginWatching(VideoId videoId)
        {
            ViewId viewId = _viewIdGenerator.GenerateViewId();
            ViewMetadatas.Add(viewId, ViewMetadataFor(videoId));
            
            return viewId;
        }

        private ViewMetadata ViewMetadataFor(VideoId videoId)
        {
            return new ViewMetadata(startedWatching: _clock.CurrentTime(), videoId);
        }

        public void StillWatching(ViewId viewId)
        {
            if (!ViewMetadatas.ContainsKey(viewId))
            {
                return;
            }
            
            if (TimeSinceStartedWatching(viewId) >= RequiredWatchLength(viewId))
            {
                CountView(viewId);
            }
        }

        private TimeSpan TimeSinceStartedWatching(ViewId viewId)
        {
            return _clock.CurrentTime() - TimeVideoStarted(viewId);
        }

        private static DateTime TimeVideoStarted(ViewId viewId)
        {
            return ViewMetadatas[viewId].StartedWatching;
        }

        private TimeSpan RequiredWatchLength(ViewId viewId)
        {
            return _videoViewQueries.VideoLength(VideoIdFor(viewId)).Multiply(0.8);
        }

        private void CountView(ViewId viewId)
        {
            _videoViewQueries.AddView(VideoIdFor(viewId));
            ViewMetadatas.Remove(viewId);
        }

        private static VideoId VideoIdFor(ViewId viewId)
        {
            return ViewMetadatas[viewId].VideoId;
        }

        private class ViewMetadata
        {
            public ViewMetadata(DateTime startedWatching, VideoId videoId)
            {
                StartedWatching = startedWatching;
                VideoId = videoId;
            }

            public DateTime StartedWatching { get; }
            
            public VideoId VideoId { get; }
        }
    }
}