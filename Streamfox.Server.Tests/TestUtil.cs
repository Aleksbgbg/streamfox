namespace Streamfox.Server.Tests
{
    using System.IO;
    using System.Text;

    using Moq;

    public static class TestUtil
    {
        public static Stream MockStream()
        {
            return new Mock<Stream>().Object;
        }

        public static byte[] ReadStreamBytes(Stream stream)
        {
            stream.Position = 0;
            return Encoding.ASCII.GetBytes(new StreamReader(stream).ReadToEnd());
        }
    }
}