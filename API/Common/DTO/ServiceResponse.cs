namespace API.Common.DTO
{
    public class ServiceResponse<T>
    {
        public bool Success { get; set; } = true;

        public int StatusCode { get; set; } = 200;
        public string? ErrorMessage { get; set; }
        public T? Data { get; set; }
    }
}
