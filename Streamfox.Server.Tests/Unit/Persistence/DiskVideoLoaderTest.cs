namespace Streamfox.Server.Tests.Unit.Persistence
{
    using System.Collections.Generic;

    using Moq;

    using Streamfox.Server.Persistence;
    using Streamfox.Server.Persistence.Operations;
    using Streamfox.Server.VideoManagement;

    using Xunit;

    public class DiskVideoLoaderTest
    {
        private readonly Mock<IFileLister> _fileLister;

        private readonly Mock<IFileReadOpener> _videoFileReadOpener;

        private readonly Mock<IFileReadOpener> _thumbnailFileReadOpener;

        private readonly DiskVideoLoader _diskVideoLoader;

        private readonly List<string> _files;

        public DiskVideoLoaderTest()
        {
            _fileLister = new Mock<IFileLister>();
            _fileLister.Setup(lister => lister.ListFiles()).Returns(() => _files.ToArray());
            _videoFileReadOpener = new Mock<IFileReadOpener>();
            _thumbnailFileReadOpener = new Mock<IFileReadOpener>();
            _diskVideoLoader = new DiskVideoLoader(
                    _fileLister.Object,
                    _videoFileReadOpener.Object,
                    _thumbnailFileReadOpener.Object);
            _files = new List<string>();
        }

        [Fact]
        public void ListLabels_ListsExistingFiles_InOrder()
        {
            SetupFilesExists(@"C:\Files\999999");
            SetupFilesExists(@"C:\Files\123456");
            SetupFilesExists(@"C:\Files\456789");

            VideoId[] videoIds = _diskVideoLoader.ListLabels();

            Assert.Equal(
                    new[] { new VideoId(123456), new VideoId(456789), new VideoId(999999) },
                    videoIds);
        }

        private void SetupFilesExists(string label)
        {
            _files.Add(label);
        }
    }
}