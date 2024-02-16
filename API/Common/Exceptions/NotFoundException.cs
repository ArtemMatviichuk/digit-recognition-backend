namespace API.Common.Exceptions
{
    public class NotFoundException : CustomException
    {
        public NotFoundException(string item)
            : base($"{item} not found")
        {
        }

        public NotFoundException(string item, CustomException innerException)
            : base($"{item} not found", innerException)
        {
        }
    }
}
