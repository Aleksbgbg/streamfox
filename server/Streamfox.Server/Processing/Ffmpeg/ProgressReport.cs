namespace Streamfox.Server.Processing.Ffmpeg
{
    public class ProgressReport
    {
        public ProgressReport(int frame)
        {
            Frame = frame;
        }

        public int Frame { get; }
    }
}