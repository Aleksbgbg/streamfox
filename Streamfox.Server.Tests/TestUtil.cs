namespace Streamfox.Server.Tests
{
    using System.IO;

    using Moq;

    public static class TestUtil
    {
        public static Stream MockStream()
        {
            return new Mock<Stream>().Object;
        }
    }
}