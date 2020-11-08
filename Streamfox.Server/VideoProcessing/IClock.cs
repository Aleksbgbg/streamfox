namespace Streamfox.Server.VideoProcessing
{
    using System;

    public interface IClock
    {
        bool IsStarted();

        void Start();

        TimeSpan ElapsedTime();
    }
}