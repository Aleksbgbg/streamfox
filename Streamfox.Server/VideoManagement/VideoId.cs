namespace Streamfox.Server.VideoManagement
{
    using System;

    public readonly struct VideoId : IEquatable<VideoId>
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

        public static bool operator ==(VideoId left, VideoId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VideoId left, VideoId right)
        {
            return !(left == right);
        }

        public bool Equals(VideoId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is VideoId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}