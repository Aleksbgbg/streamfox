namespace Streamfox.Server.Tests.Unit
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Moq;

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

        [Fact]
        public async Task SavesStreamContentsToFileWithLabelAsFilename()
        {
            byte[] bytes = { 33, 44, 55 };
            MockWriteStream fileStream = new MockWriteStream(bytes.Length);
            Stream videoStream = new MemoryStream(bytes);
            _fileSystemManipulatorMock.Setup(manipulator => manipulator.OpenFile("Hello"))
                                      .Returns(fileStream);

            await _videoSaverToDisk.SaveVideo("Hello", videoStream);

            Assert.Equal(bytes, fileStream.Bytes);
        }

        [Fact]
        public async Task DisposesFileStreamAfterWriting()
        {
            byte[] bytes = { 33, 44, 55 };
            MockWriteStream fileStream = new MockWriteStream(bytes.Length);
            Stream videoStream = new MemoryStream(bytes);
            _fileSystemManipulatorMock.Setup(manipulator => manipulator.OpenFile("Hello"))
                                      .Returns(fileStream);

            await _videoSaverToDisk.SaveVideo("Hello", videoStream);

            Assert.True(fileStream.Disposed, "File stream not disposed");
        }

        private class MockWriteStream : Stream
        {
            public MockWriteStream(int size)
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