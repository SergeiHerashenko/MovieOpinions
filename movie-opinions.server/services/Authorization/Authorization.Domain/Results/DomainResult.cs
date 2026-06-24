using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions;

namespace Authorization.Domain.Results
{
    public class DomainResult
    {
        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public Error Error { get; }

        protected DomainResult(bool isSuccess, Error error)
        {
            if(isSuccess && error != Error.None)
            {
                throw DomainInvalidInputException.InvariantViolation(
                    "Success result cannot have an error!",
                    new Dictionary<string, object>
                    {
                        ["layer"] = "Domain",
                        ["isSuccess"] = isSuccess,
                        ["expected"] = "Success => Error.None",
                        ["actual"] = $"isSuccess={isSuccess}, error != None",
                        ["error.code"] = error.Code,
                        ["error.message"] = error.Message,
                        ["violation"] = "Success result contains unexpected error"
                    }
                );
            }

            if(!isSuccess && error == Error.None)
            {
                throw DomainInvalidInputException.InvariantViolation(
                    "Failure result must have an error!",
                    new Dictionary<string, object>
                    {
                        ["layer"] = "Domain",
                        ["isSuccess"] = isSuccess,
                        ["expected"] = "Failure => Error != None",
                        ["actual"] = $"isSuccess={isSuccess}, error=NONE",
                        ["error.isNone"] = true,
                        ["violation"] = "Missing error in failure result"
                    }
                );
            }

            IsSuccess = isSuccess;
            Error = error;
        }

        public static DomainResult Success() => new(true, Error.None);

        public static DomainResult Failure(Error error) => new(false, error);
    }

    public class DomainResult<T> : DomainResult
    {
        private readonly T? _value;

        public T Value => IsSuccess
            ? _value!
            : throw DomainInvalidOperationException.ValueAccessOnFailure(
                "Cannot access value of a failed result!",
                new Dictionary<string, object>
                {
                    ["layer"] = "Domain",
                    ["isSuccess"] = IsSuccess,
                    ["expected"] = "IsSuccess == true to access Value",
                    ["actual"] = $"IsSuccess={IsSuccess}",
                    ["error"] = new
                    {
                        Error.Code,
                        Error.Message
                    },
                    ["violation"] = "Attempt to access Value on failed Result",
                    ["resultType"] = typeof(T).Name
                }
            );

        private DomainResult(T value) : base(true, Error.None) => _value = value;

        private DomainResult(Error error) : base(false, error) => _value = default;

        public static DomainResult<T> Success(T value) => new(value);

        public static new DomainResult<T> Failure(Error error) => new(error);
    }
}
