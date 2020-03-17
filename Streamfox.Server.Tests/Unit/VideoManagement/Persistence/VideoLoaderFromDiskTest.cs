namespace Streamfox.Server.Tests.Unit.VideoManagement.Persistence
{
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

        public VideoLoaderFromDiskTest()
        {
            _fileSystemCheckerMock = new Mock<IFileSystemChecker>();
            _fileSystemManipulatorMock = new Mock<IFileSystemManipulator>();
            _videoLoaderFromDisk = new VideoLoaderFromDisk(
                    _fileSystemCheckerMock.Object,
                    _fileSystemManipulatorMock.Object);
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

        private void SetupFilesExists(string label)
        {
            SetupFileExistence(label, exists: true);
        }

        private void SetupFileDoesNotExist(string label)
        {
            SetupFileExistence(label, exists: false);
        }

        private void SetupFileExistence(string label, bool exists)
        {
            _fileSystemCheckerMock.Setup(checker => checker.FileExists(label))
                                  .Returns(exists);
        }
    }
}