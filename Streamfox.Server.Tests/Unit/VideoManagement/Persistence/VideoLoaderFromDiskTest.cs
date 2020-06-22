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
        private readonly Mock<IFileSystemChecker> _fileSystemCheckerMock;

        private readonly Mock<IFileSystemManipulator> _fileSystemManipulatorMock;

        private readonly VideoLoaderFromDisk _videoLoaderFromDisk;

        private readonly List<string> _files;

        public VideoLoaderFromDiskTest()
        {
            _fileSystemCheckerMock = new Mock<IFileSystemChecker>();
            _fileSystemCheckerMock.Setup(checker => checker.FileExists(It.IsAny<string>()))
                                  .Returns<string>(name => _files.Contains(name));
            _fileSystemCheckerMock.Setup(checker => checker.ListFiles())
                                  .Returns(() => _files.ToArray());
            _fileSystemManipulatorMock = new Mock<IFileSystemManipulator>();
            _videoLoaderFromDisk = new VideoLoaderFromDisk(
                    _fileSystemCheckerMock.Object,
                    _fileSystemManipulatorMock.Object);
            _files = new List<string>();
        }

        [Fact]
        public void LoadsExistingLabel()
        {
            SetupFilesExists("ExistingLabel");
            Stream fileStream = new MemoryStream(new byte[] { 33, 44, 55 });
            _fileSystemManipulatorMock.Setup(manipulator => manipulator.OpenFile("ExistingLabel"))
                                      .Returns(fileStream);

            Optional<Stream> loadedStream = _videoLoaderFromDisk.LoadVideo("ExistingLabel");

            Assert.True(loadedStream.HasValue, "No video loaded");
            Assert.Equal(fileStream, loadedStream.Value);
        }

        [Fact]
        public void ReturnsEmptyWhenLabelDoesNotExist()
        {
            SetupFileDoesNotExist("NonExistingLabel");

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

        private void SetupFileDoesNotExist(string label)
        {
        }
    }
}