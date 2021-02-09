namespace Streamfox.Server.Controllers
{
    using System;

    public readonly struct ViewId : IEquatable<ViewId>
    {
        public ViewId(long value)
        {
            Value = value;
        }

        public long Value { get; }

        public bool Equals(ViewId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is ViewId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(ViewId left, ViewId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ViewId left, ViewId right)
        {
            return !left.Equals(right);
        }
    }
}