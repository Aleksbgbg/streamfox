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
            _videoLoaderFromDisk = new VideoLoaderFromDisk(_fileSystemCheckerMock.Object, _fileSystemManipulatorMock.Object);
        }

        [Fact]
        public void LoadsExistingLabel()
        {
            Stream fileStream = new MemoryStream(new byte[] { 33, 44, 55 });
            _fileSystemCheckerMock.Setup(checker => checker.FileExists("ExistingLabel"))
                                  .Returns(true);
            _fileSystemManipulatorMock.Setup(manipulator => manipulator.OpenFile("ExistingLabel"))
                                      .Returns(fileStream);

            Optional<Stream> loadedStream = _videoLoaderFromDisk.LoadVideo("ExistingLabel");

            Assert.True(loadedStream.HasValue, "No video loaded");
            Assert.Equal(fileStream, loadedStream.Value);
        }

        [Fact]
        public void ReturnsEmptyWhenLabelDoesNotExist()
        {
            _fileSystemCheckerMock.Setup(checker => checker.FileExists("NonExistingLabel"))
                                  .Returns(false);

            Optional<Stream> loadedStream = _videoLoaderFromDisk.LoadVideo("NonExistingLabel");

            Assert.False(loadedStream.HasValue, "Video loaded when label does not exist");
        }
    }
}