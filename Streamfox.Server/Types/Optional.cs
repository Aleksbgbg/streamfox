namespace Streamfox.Server.Types
{
    public readonly struct Optional<T>
    {
        private Optional(T value)
        {
            HasValue = true;
            Value = value;
        }

        public bool HasValue { get; }

        public T Value { get; }

        public static Optional<T> Of(T value)
        {
            return new Optional<T>(value);
        }

        public static Optional<T> Empty()
        {
            return new Optional<T>();
        }
    }

    public static class Optional
    {
        public static Optional<T> Of<T>(T value)
        {
            return Optional<T>.Of(value);
        }
    }
}