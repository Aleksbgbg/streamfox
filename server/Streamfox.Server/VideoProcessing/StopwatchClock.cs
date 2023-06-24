namespace Streamfox.Server.VideoProcessing
{
    using System;
    using System.Diagnostics;

    public class StopwatchClock : IClock
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public bool IsStarted()
        {
            return _stopwatch.IsRunning;
        }

        public void Start()
        {
            _stopwatch.Start();
        }

        public TimeSpan ElapsedTime()
        {
            return _stopwatch.Elapsed;
        }
    }
}