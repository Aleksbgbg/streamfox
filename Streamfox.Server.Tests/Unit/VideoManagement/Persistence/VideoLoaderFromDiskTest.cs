namespace Streamfox.Server.Tests.Unit.VideoManagement.Persistence
{
    using System.Collections.Generic;
    using System.IO;

    using Moq;

    using Streamfox.Server.Types;
    using Streamfox.Server.VideoManagement.Persistence;

    using Xunit;

    public class VideoLoaderFromDiskTest
    {
        private readonly Mock<IVideoFileContainer> _videoFileContainerMock;

        private readonly Mock<IVideoFileReader> _videoFileReaderMock;

        private readonly Mock<IThumbnailFileReader> _thumbnailFileReaderMock;

        private readonly VideoLoaderFromDisk _videoLoaderFromDisk;

        private readonly List<string> _files;

        public VideoLoaderFromDiskTest()
        {
            _videoFileContainerMock = new Mock<IVideoFileContainer>();
            _videoFileContainerMock.Setup(container => container.FileExists(It.IsAny<string>()))
                                   .Returns<string>(name => _files.Contains(name));
            _videoFileContainerMock.Setup(container => container.ListFiles())
                                   .Returns(() => _files.ToArray());
            _videoFileReaderMock = new Mock<IVideoFileReader>();
            _thumbnailFileReaderMock = new Mock<IThumbnailFileReader>();
            _videoLoaderFromDisk = new VideoLoaderFromDisk(
                    _videoFileContainerMock.Object,
                    _videoFileReaderMock.Object,
                    _thumbnailFileReaderMock.Object);
            _files = new List<string>();
        }

        [Fact]
        public void LoadsExistingLabel()
        {
            SetupFilesExists("ExistingLabel");
            Stream fileStream = new MemoryStream(new byte[] { 33, 44, 55 });
            _videoFileReaderMock.Setup(reader => reader.OpenRead("ExistingLabel"))
                                .Returns(fileStream);

            Optional<Stream> loadedStream = _videoLoaderFromDisk.LoadVideo("ExistingLabel");

            Assert.True(loadedStream.HasValue, "No video loaded");
            Assert.Equal(fileStream, loadedStream.Value);
        }

        [Fact]
        public void ReturnsEmptyWhenLabelDoesNotExist()
        {
            Optional<Stream> loadedStream = _videoLoaderFromDisk.LoadVideo("NonExistingLabel");

            Assert.False(loadedStream.HasValue, "Video loaded when label does not exist");
        }

        [Fact]
        public void ListsExistingFilesAsLabels()
        {
            SetupFilesExists(@"C:\Files\123456");
            SetupFilesExists(@"C:\Files\456789");

            string[] labels = _videoLoaderFromDisk.ListLabels();

            Assert.Equal(new[] { "123456", "456789" }, labels);
        }

        private void SetupFilesExists(string label)
        {
            _files.Add(label);
        }
    }
}