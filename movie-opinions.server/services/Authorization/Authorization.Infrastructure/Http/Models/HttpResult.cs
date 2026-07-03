namespace Authorization.Infrastructure.Http.Models
{
    public class HttpResult<T>
    {
        public bool IsSuccess { get; init; }

        public T? Data { get; init; }

        public string? ErrorMessage { get; init; }

        public int StatusCode { get; init; }

        public static HttpResult<T> Success(T? data, int statusCode) => new() { IsSuccess = true, Data = data, StatusCode = statusCode };

        public static HttpResult<T> Failure(string error, int statusCode) => new() { IsSuccess = false, ErrorMessage = error, StatusCode = statusCode };
    }
}
