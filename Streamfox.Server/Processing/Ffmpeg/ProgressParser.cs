namespace Streamfox.Server.Processing.Ffmpeg
{
    using System;

    using Streamfox.Server.Types;

    public static class ProgressParser
    {
        public static Optional<ProgressReport> ParseProgress(string line)
        {
            if (!line.StartsWith("frame="))
            {
                return Optional<ProgressReport>.Empty();
            }

            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            int frames = int.Parse(parts[1]);

            return Optional.Of(new ProgressReport(frames));
        }
    }
}