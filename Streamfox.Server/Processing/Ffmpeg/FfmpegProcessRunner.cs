namespace Streamfox.Server.Processing.Ffmpeg
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Streamfox.Server.Types;
    using Streamfox.Server.VideoProcessing;

    public class FfmpegProcessRunner : IFfmpegProcessRunner
    {
        public async Task<IProgressLogger> RunFfmpeg(string args)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo("ffmpeg", args)
                {
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            FfmpegProgressLogger ffmpegProgressLogger = new FfmpegProgressLogger(cancellationTokenSource.Token);

            Task _ = process.StartAsync();
            Task __ = Task.Run(
                    async () =>
                    {
                        using (process.StandardError)
                        {
                            while (!process.StandardError.EndOfStream)
                            {
                                string line = await process.StandardError.ReadLineAsync();

                                Optional<ProgressReport> progressReport =
                                        ProgressParser.ParseProgress(line);

                                if (progressReport.HasValue)
                                {
                                    ffmpegProgressLogger.AddProgressReport(progressReport.Value);
                                }
                            }

                            ffmpegProgressLogger.AddProgressReport(
                                    new ProgressReport(int.MaxValue));
                        }

                        cancellationTokenSource.Cancel();
                    });

            return ffmpegProgressLogger;
        }

        public async Task<string> RunFfprobe(string args)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo("ffprobe", args)
                {
                    RedirectStandardOutput = true
                },
                EnableRaisingEvents = true
            };

            MemoryStream output = new MemoryStream();

            Task runProcess = process.StartAsync();
            Task readOutput = Task.Run(
                    async () =>
                    {
                        using (process.StandardOutput)
                        {
                            await process.StandardOutput.BaseStream.CopyToAsync(output);
                        }
                    });

            await Task.WhenAll(runProcess, readOutput);

            output.Position = 0;
            using StreamReader outputReader = new StreamReader(output);
            return await outputReader.ReadToEndAsync();
        }

        private class FfmpegProgressLogger : IProgressLogger
        {
            private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);

            private readonly ConcurrentQueue<ProgressReport> _progressReports =
                    new ConcurrentQueue<ProgressReport>();

            private readonly CancellationToken _cancellationToken;

            public FfmpegProgressLogger(CancellationToken cancellationToken)
            {
                _cancellationToken = cancellationToken;
            }

            public void AddProgressReport(ProgressReport progressReport)
            {
                _progressReports.Enqueue(progressReport);
                _semaphore.Release();
            }

            public async Task<bool> HasMoreProgress()
            {
                await _semaphore.WaitAsync(_cancellationToken);
                return !_cancellationToken.IsCancellationRequested;
            }

            public Task<ProgressReport> GetNextProgress()
            {
                if (_progressReports.TryDequeue(out ProgressReport result))
                {
                    return Task.FromResult(result);
                }

                throw new InvalidOperationException("Could not dequeue progress report.");
            }
        }
    }
}