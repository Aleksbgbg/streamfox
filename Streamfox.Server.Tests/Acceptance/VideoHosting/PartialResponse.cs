namespace Streamfox.Server.Tests.Acceptance.VideoHosting
{
    using System.Net.Http.Headers;

    public class PartialResponse
    {
        private PartialResponse(
                byte[] bytes, long contentLength, long start, long end, long totalSize)
        {
            Bytes = bytes;
            ContentLength = contentLength;
            Start = start;
            End = end;
            TotalSize = totalSize;
        }

        public byte[] Bytes { get; }

        public long ContentLength { get; }

        public long Start { get; }

        public long End { get; }

        public long TotalSize { get; }

        public static PartialResponse Parse(HttpContentHeaders headers, byte[] bytes)
        {
            long contentLength = headers.ContentLength.Value;

            long start = headers.ContentRange.From.Value;
            long end = headers.ContentRange.To.Value;
            long size = headers.ContentRange.Length.Value;

            return new PartialResponse(bytes, contentLength, start, end, size);
        }
    }
}