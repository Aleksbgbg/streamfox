namespace Streamfox.Server
{
    public class VideoId
    {
        public VideoId(long value)
        {
            Value = value;
        }

        public long Value { get; }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}