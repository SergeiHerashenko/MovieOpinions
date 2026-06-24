namespace Authorization.Response
{
    public class ErrorResponse
    {
        public bool IsSuccess { get; set; }

        public List<string> Messages { get; set; } = new();

        public int StatusCode { get; set; }

        public List<string> ErrorCode { get; set; } = [];
    }
}
