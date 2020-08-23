namespace Streamfox.Server.Tests.Unit.Persistence
{
    using System.Collections.Generic;
    using System.IO;

    using Moq;

    using Streamfox.Server.Persistence;
    using Streamfox.Server.Persistence.Operations;
    using Streamfox.Server.Types;

    using Xunit;

    public class DiskVideoLoaderTest
    {
        private readonly Mock<IFileLister> _fileLister;

        private readonly Mock<IFileExistenceChecker> _fileExistenceChecker;

        private readonly Mock<IFileReadOpener> _videoFileReadOpener;

        private readonly Mock<IFileReadOpener> _thumbnailFileReadOpener;

        private readonly DiskVideoLoader _diskVideoLoader;

        private readonly List<string> _files;

        public DiskVideoLoaderTest()
        {
            _fileLister = new Mock<IFileLister>();
            _fileExistenceChecker = new Mock<IFileExistenceChecker>();
            _videoFileReadOpener = new Mock<IFileReadOpener>();
            _thumbnailFileReadOpener = new Mock<IFileReadOpener>();

            _fileLister.Setup(lister => lister.ListFiles()).Returns(() => _files.ToArray());
            _fileExistenceChecker.Setup(checker => checker.Exists(It.IsAny<string>()))
                                 .Returns<string>(name => _files.Contains(name));
            _diskVideoLoader = new DiskVideoLoader(
                    _fileLister.Object,
                    _fileExistenceChecker.Object,
                    _videoFileReadOpener.Object,
                    _thumbnailFileReadOpener.Object);
            _files = new List<string>();
        }

        [Fact]
        public void LoadVideo_LoadsExistingLabel()
        {
            SetupFilesExists("ExistingLabel");
            Stream fileStream = new MemoryStream(new byte[] { 33, 44, 55 });
            _videoFileReadOpener.Setup(reader => reader.OpenRead("ExistingLabel"))
                                .Returns(fileStream);

            Optional<Stream> loadedStream = _diskVideoLoader.LoadVideo("ExistingLabel");

            Assert.True(loadedStream.HasValue, "No video loaded");
            Assert.Equal(fileStream, loadedStream.Value);
        }

        [Fact]
        public void LoadVideo_ReturnsEmptyWhenLabelDoesNotExist()
        {
            Optional<Stream> loadedStream = _diskVideoLoader.LoadVideo("NonExistingLabel");

            Assert.False(loadedStream.HasValue, "Video loaded when label does not exist");
        }

        [Fact]
        public void ListLabels_ListsExistingFiles_FilenamesOnly()
        {
            SetupFilesExists(@"C:\Files\123456");
            SetupFilesExists(@"C:\Files\456789");

            string[] labels = _diskVideoLoader.ListLabels();

            Assert.Equal(new[] { "123456", "456789" }, labels);
        }

        [Fact]
        public void LoadThumbnail_LoadsExistingLabel()
        {
            SetupFilesExists("ExistingLabel");
            Stream fileStream = new MemoryStream(new byte[] { 55, 44, 33 });
            _thumbnailFileReadOpener.Setup(reader => reader.OpenRead("ExistingLabel"))
                                    .Returns(fileStream);

            Optional<Stream> loadedStream = _diskVideoLoader.LoadThumbnail("ExistingLabel");

            Assert.True(loadedStream.HasValue, "No thumbnail loaded");
            Assert.Equal(fileStream, loadedStream.Value);
        }

        [Fact]
        public void LoadThumbnail_ReturnsEmptyWhenLabelDoesNotExist()
        {
            Optional<Stream> loadedStream = _diskVideoLoader.LoadThumbnail("NonExistingLabel");

            Assert.False(loadedStream.HasValue, "Thumbnail loaded when label does not exist");
        }

        private void SetupFilesExists(string label)
        {
            _files.Add(label);
        }
    }
}