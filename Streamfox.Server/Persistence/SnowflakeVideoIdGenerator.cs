namespace Streamfox.Server.Persistence
{
    using IdGen;

    using Streamfox.Server.VideoManagement;

    public class SnowflakeVideoIdGenerator : IVideoIdGenerator
    {
        private static readonly IdGenerator IdGenerator = new IdGenerator(0);

        public VideoId GenerateVideoId()
        {
            return new VideoId(IdGenerator.CreateId());
        }
    }
}