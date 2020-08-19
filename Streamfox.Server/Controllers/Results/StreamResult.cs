namespace Streamfox.Server.Controllers.Results
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    using Streamfox.Server.Types;

    public class StreamResult : IActionResult
    {
        private const int BufferSize = 81_920;

        private const int ChunkSize = 1024 * 1024 * 5;

        public StreamResult(Stream stream) : this(stream, "application/octet-stream")
        {
        }

        public StreamResult(Stream stream, string contentType)
        {
            Stream = stream;
            ContentType = contentType;
        }

        public Stream Stream { get; }

        public string ContentType { get; }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            HttpRequest request = context.HttpContext.Request;
            HttpResponse response = context.HttpContext.Response;

            response.ContentType = ContentType;

            if (request.Headers.ContainsKey("Range"))
            {
                Optional<Range> optionalRange = ParseRange(request.Headers["Range"].First());

                if (optionalRange.HasValue)
                {
                    Range range = optionalRange.Value;
                    range.End = Math.Min(range.End, Stream.Length - 1);

                    response.StatusCode = 206;
                    response.ContentLength = range.Length;
                    response.Headers.Add(
                            "Content-Range",
                            $"bytes {range.Start}-{range.End}/{Stream.Length}");

                    long bytesWritten = 0;
                    long bytesToWrite = range.Length;
                    byte[] buffer = new byte[BufferSize];

                    Stream.Seek(range.Start, SeekOrigin.Begin);

                    while (bytesWritten < bytesToWrite)
                    {
                        int bytesRead = await Stream.ReadAsync(
                                buffer,
                                0,
                                (int)Math.Min(buffer.Length, bytesToWrite - bytesWritten));
                        await response.Body.WriteAsync(buffer, 0, bytesRead);

                        bytesWritten += bytesRead;
                    }

                    return;
                }
            }

            await Stream.CopyToAsync(context.HttpContext.Response.Body);
        }

        private struct Range
        {
            public Range(long start, long end)
            {
                Start = start;
                End = end;
            }

            public long Start { get; set; }

            public long End { get; set; }

            public long Length => (End - Start) + 1;
        }

        private static Optional<Range> ParseRange(string range)
        {
            const string startString = "bytes=";

            if (!range.StartsWith(startString))
            {
                return EmptyRange();
            }

            return ParseRangeNoPrefix(range.Substring(startString.Length));
        }

        private static Optional<Range> ParseRangeNoPrefix(string range)
        {
            long start;
            long end;

            if (range.EndsWith('-'))
            {
                range = range.Substring(0, range.Length - 1);

                Optional<int> optionalStart = TryParse(range);

                if (!optionalStart.HasValue)
                {
                    return EmptyRange();
                }

                start = optionalStart.Value;
                end = start + ChunkSize - 1;
            }
            else
            {
                string[] parts = range.Split('-');

                if (parts.Length != 2)
                {
                    return EmptyRange();
                }

                Optional<int> optionalStart = TryParse(parts[0]);
                Optional<int> optionalEnd = TryParse(parts[1]);

                if (!optionalStart.HasValue || !optionalEnd.HasValue)
                {
                    return EmptyRange();
                }

                start = optionalStart.Value;
                end = optionalEnd.Value;
            }

            if ((start < 0) || (start > end))
            {
                return EmptyRange();
            }

            return Optional.Of(new Range(start, end));
        }

        private static Optional<Range> EmptyRange()
        {
            return Optional<Range>.Empty();
        }

        private static Optional<int> TryParse(string s)
        {
            if (int.TryParse(s, out int value))
            {
                return Optional.Of(value);
            }

            return Optional<int>.Empty();
        }
    }
}