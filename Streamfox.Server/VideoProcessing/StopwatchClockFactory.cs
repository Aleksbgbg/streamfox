namespace Streamfox.Server.VideoProcessing
{
    public class StopwatchClockFactory : IClockFactory
    {
        public IClock CreateClock()
        {
            return new StopwatchClock();
        }
    }
}