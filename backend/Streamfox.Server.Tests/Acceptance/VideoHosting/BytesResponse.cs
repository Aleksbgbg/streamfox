namespace Streamfox.Server.Tests.Acceptance.VideoHosting
{
    public class BytesResponse
    {
        public BytesResponse(byte[] bytes, string contentType, bool isSuccess, string errorReason)
        {
            Bytes = bytes;
            ContentType = contentType;
            IsSuccess = isSuccess;
            ErrorReason = errorReason;
        }

        public bool IsSuccess { get; }

        public string ErrorReason { get; }

        public byte[] Bytes { get; }

        public string ContentType { get; }
    }
}