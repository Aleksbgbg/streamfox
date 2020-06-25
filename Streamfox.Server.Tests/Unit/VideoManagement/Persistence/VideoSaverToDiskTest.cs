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
        private readonly Mock<IVideoFileWriter> _videoFileWriterMock;

        private readonly Mock<IThumbnailFileWriter> _thumbnailFileWriterMock;

        private readonly VideoSaverToDisk _videoSaverToDisk;

        public VideoSaverToDiskTest()
        {
            _videoFileWriterMock = new Mock<IVideoFileWriter>();
            _videoFileWriterMock.Setup(writer => writer.OpenWrite(It.IsAny<string>()))
                                .Returns(Stream.Null);
            _thumbnailFileWriterMock = new Mock<IThumbnailFileWriter>();
            _thumbnailFileWriterMock.Setup(writer => writer.OpenWrite(It.IsAny<string>()))
                                .Returns(Stream.Null);
            _videoSaverToDisk = new VideoSaverToDisk(
                    _videoFileWriterMock.Object,
                    _thumbnailFileWriterMock.Object);
        }

        [Theory]
        [InlineData(new byte[] { 1, 2, 3 })]
        [InlineData(new byte[] { 4, 5, 6 })]
        public async Task SavesVideoStreamToFileWithLabelAsFilename(byte[] bytes)
        {
            Stream videoStream = new MemoryStream(bytes);
            MockWriteStream fileStream = new MockWriteStream(bytes.Length);
            SetupVideoStreamForLabel(fileStream, "VideoLabel");

            await _videoSaverToDisk.SaveVideo("VideoLabel", videoStream, Stream.Null);

            Assert.Equal(bytes, fileStream.Bytes);
        }

        [Theory]
        [InlineData(new byte[] { 1, 2, 3 })]
        [InlineData(new byte[] { 4, 5, 6 })]
        public async Task SavesThumbnailStreamToFileWithLabelAsFilename(byte[] bytes)
        {
            Stream thumbnailStream = new MemoryStream(bytes);
            MockWriteStream fileStream = new MockWriteStream(bytes.Length);
            SetupThumbnailStreamForLabel(fileStream, "VideoLabel");

            await _videoSaverToDisk.SaveVideo("VideoLabel", Stream.Null, thumbnailStream);

            Assert.Equal(bytes, fileStream.Bytes);
        }

        [Fact]
        public async Task DisposesWrittenVideoFileStream()
        {
            MockWriteStream videoStream = new MockWriteStream();
            SetupVideoStreamForLabel(videoStream, "VideoLabel");

            await _videoSaverToDisk.SaveVideo("VideoLabel", Stream.Null, Stream.Null);

            Assert.True(videoStream.Disposed, "File stream not disposed");
        }

        [Fact]
        public async Task DisposesWrittenThumbnailFileStream()
        {
            MockWriteStream thumbnailStream = new MockWriteStream();
            SetupThumbnailStreamForLabel(thumbnailStream, "VideoLabel");

            await _videoSaverToDisk.SaveVideo("VideoLabel", Stream.Null, Stream.Null);

            Assert.True(thumbnailStream.Disposed, "File stream not disposed");
        }

        private void SetupVideoStreamForLabel(Stream stream, string videoLabel)
        {
            _videoFileWriterMock.Setup(writer => writer.OpenWrite(videoLabel))
                                .Returns(stream);
        }

        private void SetupThumbnailStreamForLabel(Stream stream, string videoLabel)
        {
            _thumbnailFileWriterMock.Setup(writer => writer.OpenWrite(videoLabel))
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