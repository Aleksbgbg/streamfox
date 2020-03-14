namespace Streamfox.Server.Types
{
    public class Optional<T>
    {
        private Optional()
        {
            HasValue = false;
        }

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
}