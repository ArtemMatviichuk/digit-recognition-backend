namespace API.Common.Exceptions
{
    public class ValidationException : CustomException
    {
        public override int ErrorCode => 400;

        public ValidationException() { }

        public ValidationException(string message)
            : base(message) { }

        public ValidationException(string message, CustomException innerException)
            : base(message, innerException) { }
    }
}
