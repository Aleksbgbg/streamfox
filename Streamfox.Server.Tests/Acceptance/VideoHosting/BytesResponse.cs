namespace Streamfox.Server.Tests.Acceptance.VideoHosting
{
    public class BytesResponse
    {
        public BytesResponse(byte[] bytes, string contentType)
        {
            Bytes = bytes;
            ContentType = contentType;
        }

        public byte[] Bytes { get; }

        public string ContentType { get; }
    }
}