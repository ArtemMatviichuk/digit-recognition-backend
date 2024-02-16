namespace API.Common.Exceptions
{
    public class CustomException : Exception
    {
        public virtual int ErrorCode { get; }

        public CustomException() { }

        public CustomException(string message)
            : base(message) { }

        public CustomException(string message, CustomException innerException)
            : base(message, innerException) { }
    }
}
