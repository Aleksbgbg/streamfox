namespace Streamfox.Server.Utilities
{
    using System.IO;
    using System.Threading.Tasks;

    public static class Streams
    {
        public static async Task<MemoryStream> CloneToMemory(Stream stream)
        {
            MemoryStream clone = new MemoryStream();
            await stream.CopyToAsync(clone);
            clone.Position = 0;
            return clone;
        }
    }
}