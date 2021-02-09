namespace Streamfox.Server.Persistence
{
    using IdGen;
    using Streamfox.Server.Controllers;
    using Streamfox.Server.VideoManagement;

    public class SnowflakeIdGenerator : IVideoIdGenerator, IViewIdGenerator
    {
        private static readonly IdGenerator IdGenerator = new IdGenerator(0);

        public VideoId GenerateVideoId()
        {
            return new VideoId(IdGenerator.CreateId());
        }

        public ViewId GenerateViewId()
        {
            return new ViewId(IdGenerator.CreateId());
        }
    }
}