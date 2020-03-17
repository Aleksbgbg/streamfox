namespace Streamfox.Server.Tests.Unit.VideoManagement.Persistence
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Moq;

    using Streamfox.Server.VideoManagement.Persistence;

    using Xunit;

    public class VideoSaverToDiskTest
    {
        private readonly Mock<IFileSystemManipulator> _fileSystemManipulatorMock;

        private readonly VideoSaverToDisk _videoSaverToDisk;

        public VideoSaverToDiskTest()
        {
            _fileSystemManipulatorMock = new Mock<IFileSystemManipulator>();
            _videoSaverToDisk = new VideoSaverToDisk(_fileSystemManipulatorMock.Object);
        }

        [Theory]
        [InlineData(new byte[] { 1, 2, 3 })]
        [InlineData(new byte[] { 4, 5, 6 })]
        public async Task SavesStreamContentsToFileWithLabelAsFilename(byte[] bytes)
        {
            Stream videoStream = new MemoryStream(bytes);
            MockWriteStream fileStream = new MockWriteStream(bytes.Length);
            SetupFileStreamForVideoLabel(fileStream, "VideoLabel");

            await _videoSaverToDisk.SaveVideo("VideoLabel", videoStream);

            Assert.Equal(bytes, fileStream.Bytes);
        }

        [Fact]
        public async Task DisposesFileStream()
        {
            MockWriteStream fileStream = new MockWriteStream();
            SetupFileStreamForVideoLabel(fileStream, "VideoLabel");

            await _videoSaverToDisk.SaveVideo("VideoLabel", Stream.Null);

            Assert.True(fileStream.Disposed, "File stream not disposed");
        }

        private void SetupFileStreamForVideoLabel(Stream stream, string videoLabel)
        {
            _fileSystemManipulatorMock.Setup(manipulator => manipulator.OpenFile(videoLabel))
                                      .Returns(stream);
        }

        private class MockWriteStream : Stream
        {
            public MockWriteStream(int size = 0)
            {
                Bytes = new byte[size];
            }

            public byte[] Bytes { get; }

            public bool Disposed { get; private set; }

            public override bool CanRead => false;

            public override bool CanSeek => false;

            public override bool CanWrite => true;

            public override long Length => Bytes.Length;

            public override long Position { get; set; } = 0L;

            protected override void Dispose(bool disposing)
            {
                Disposed = true;
            }

            public override void Flush()
            {
                throw new NotImplementedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                for (int index = 0; index < count; ++index)
                {
                    Bytes[index + offset] = buffer[index];
                }
            }
        }
    }
}