using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Domain.Common.Errors
{
    public sealed class Error
    {
        public string Code { get; }

        public string Message { get; }

        public ErrorType ErrorType { get; }

        public Error(string code, string message, ErrorType errorType)
        {
            Code = code;
            Message = message;
            ErrorType = errorType;
        }
    }
}
