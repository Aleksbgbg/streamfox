namespace Streamfox.Server.Controllers
{
    using System;
    using Streamfox.Server.VideoManagement;

    public interface IVideoViewQueries
    {
        void AddView(VideoId videoId);

        int GetViews(VideoId videoId);

        TimeSpan VideoLength(VideoId videoId);
    }
}