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
        public Task RunFfmpeg(string args)
        {
            return new Process
            {
                StartInfo = new ProcessStartInfo("ffmpeg", args),
                EnableRaisingEvents = true
            }.StartAsync();
        }

        public Task<IProgressLogger> RunFfmpegWithProgressLogging(string args)
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
            FfmpegProgressLogger ffmpegProgressLogger =
                    new FfmpegProgressLogger(cancellationTokenSource.Token);

            Task _ = process.StartAsync();
            Task.Run(
                    async () =>
                    {
                        ffmpegProgressLogger.AddProgressReport(new ProgressReport(0));

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
                        }

                        ffmpegProgressLogger.AddProgressReport(new ProgressReport(int.MaxValue));
                        cancellationTokenSource.Cancel();
                    });

            return Task.FromResult<IProgressLogger>(ffmpegProgressLogger);
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