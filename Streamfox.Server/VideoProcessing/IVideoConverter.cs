namespace Streamfox.Server.VideoProcessing
{
    using System.Threading.Tasks;

    public interface IVideoConverter
    {
        void RunConversionTask(Task task);
    }
}