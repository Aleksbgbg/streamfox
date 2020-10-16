namespace Streamfox.Server.Processing.Ffmpeg
{
    using System;

    public static class ProgressParser
    {
        public static ProgressReport ParseProgress(string line)
        {
            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            int frames = int.Parse(parts[1]);

            return new ProgressReport(frames);
        }
    }
}