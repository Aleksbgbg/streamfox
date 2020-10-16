namespace Streamfox.Server.Processing
{
    using System.Threading.Tasks;

    public interface IVideoCoercer
    {
        Task CoerceToVp9(string sourcePath, string targetPath);

        Task CopyWithoutCoercing(string sourcePath, string targetPath);
    }
}