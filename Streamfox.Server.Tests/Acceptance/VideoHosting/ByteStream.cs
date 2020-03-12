namespace Streamfox.Server.Tests.Acceptance.VideoHosting
{
    public class ByteStream
    {
        public ByteStream(byte[] bytes)
        {
            Bytes = bytes;
        }

        public byte[] Bytes { get; }
    }
}