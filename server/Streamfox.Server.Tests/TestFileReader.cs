namespace Streamfox.Server.Tests
{
    using System.IO;
    using System.Threading.Tasks;

    public static class TestFileReader
    {
        public static string ReadString(string directory, string name)
        {
            return File.ReadAllText(PathToFile(directory, name));
        }

        public static Task<byte[]> ReadBytesAsync(string directory, string name)
        {
            return File.ReadAllBytesAsync(PathToFile(directory, name));
        }

        private static string PathToFile(string directory, string name)
        {
            return $"TestFiles/{directory}/{name}";
        }
    }
}