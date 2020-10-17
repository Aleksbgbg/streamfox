namespace Streamfox.Server.Processing
{
    using System.Threading.Tasks;

    using Streamfox.Server.VideoProcessing;

    public interface IVideoCoercer
    {
        Task<IProgressLogger> CoerceToVp9(string sourcePath, string targetPath);

        Task<IProgressLogger> CopyWithoutCoercing(string sourcePath, string targetPath);
    }
}