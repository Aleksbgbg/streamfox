namespace Streamfox.Server.VideoManagement.Processing
{
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;

    public class FfmpegProcessVideoSnapshotter : IVideoSnapshotter
    {
        private const int BufferSize = 81_920;

        public async Task<Stream> ProduceVideoSnapshot(Stream video)
        {
            video.Position = 0;

            Process process = Process.Start(
                    new ProcessStartInfo("ffmpeg", "-i pipe: -vframes 1 -q:v 2 pipe:.jpg")
                    {
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    });

            await using (process.StandardInput.BaseStream)
            {
                int iterations = ((int)(video.Length / BufferSize)) / 100;

                for (int index = 0; index < iterations; ++index)
                {
                    byte[] buffer = new byte[BufferSize];

                    int read = video.Read(buffer);
                    process.StandardInput.BaseStream.Write(buffer, 0, read);
                }
            }

            MemoryStream outputStream = new MemoryStream();

            await using (process.StandardOutput.BaseStream)
            {
                await process.StandardOutput.BaseStream.CopyToAsync(outputStream);
            }

            process.WaitForExit();

            video.Position = 0;
            outputStream.Position = 0;

            return outputStream;
        }
    }
}