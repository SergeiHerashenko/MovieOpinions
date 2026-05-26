using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions;

namespace Authorization.Domain.Results
{
    public class Result
    {
        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public Error Error { get; }

        protected Result(bool isSuccess, Error error)
        {
            if(isSuccess && error != Error.None)
            {
                throw DomainInvalidInputException.InvariantViolation(
                    "Success result cannot have an error!",
                    new Dictionary<string, object>
                    {
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

        public static Result Success() => new(true, Error.None);

        public static Result Failure(Error error) => new(false, error);
    }

    public class Result<T> : Result
    {
        private readonly T? _value;

        public T Value => IsSuccess
            ? _value!
            : throw DomainInvalidOperationException.ValueAccessOnFailure(
                "Cannot access value of a failed result!",
                new Dictionary<string, object>
                {
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

        private Result(T value) : base(true, Error.None) => _value = value;

        private Result(Error error) : base(false, error) => _value = default;

        public static Result<T> Success(T value) => new(value);

        public static new Result<T> Failure(Error error) => new(error);
    }
}
