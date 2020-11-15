namespace Streamfox.Server.VideoManagement
{
    public interface IVideoLister
    {
        VideoId[] ListLabels();
    }
}