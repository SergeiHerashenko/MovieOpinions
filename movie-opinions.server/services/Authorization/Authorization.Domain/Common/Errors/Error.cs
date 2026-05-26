namespace Authorization.Domain.Common.Errors
{
    public sealed class Error
    {
        public string Code { get; }

        public string Message { get; }

        public ErrorType Type { get; }

        public Error(string code, string message, ErrorType type)
        {
            Code = code;
            Message = message;
            Type = type;
        }

        public static readonly Error None = new(string.Empty, string.Empty, default);
    }
}
