namespace Streamfox.Server
{
    public readonly struct VideoId
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