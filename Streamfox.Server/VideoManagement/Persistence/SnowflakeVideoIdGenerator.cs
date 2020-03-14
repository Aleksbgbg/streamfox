namespace Streamfox.Server.VideoManagement.Persistence
{
    using IdGen;

    public class SnowflakeVideoIdGenerator : IVideoIdGenerator
    {
        private static readonly IdGenerator IdGenerator = new IdGenerator(0);

        public VideoId GenerateVideoId()
        {
            return new VideoId(IdGenerator.CreateId());
        }
    }
}