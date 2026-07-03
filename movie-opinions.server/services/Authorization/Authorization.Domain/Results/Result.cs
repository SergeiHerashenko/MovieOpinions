using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.DomainException;

namespace Authorization.Domain.Results
{
    public class Result
    {
        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public IReadOnlyCollection<Error> Errors { get; }

        protected Result(bool isSuccess, IEnumerable<Error> errors)
        {
            var errorList = errors.ToList();

            if (isSuccess && errorList.Any())
            {
                throw DomainInvariantViolationException.BrokenState<Result>(
                    "Success result cannot have an errors!",
                    new Dictionary<string, object?>
                    {
                        ["IsSuccess"] = isSuccess,
                        ["Expected"] = "Success => Errors.Count == 0",
                        ["Actual"] = $"isSuccess={isSuccess}, errors.Count={errorList.Count}",
                        ["Errors"] = errorList.Select(e => new
                        {
                            e.Code,
                            e.Message,
                            Type = e.Type.ToString()
                        }).ToArray(),
                        ["Violation"] = "The result of successful execution contains unexpected errors."
                    }
                );
            }

            if(!isSuccess && !errorList.Any())
            {
                throw DomainInvariantViolationException.BrokenState<Result>(
                    "The failure result must contain at least one error.!",
                    new Dictionary<string, object?>
                    {
                        ["IsSuccess"] = isSuccess,
                        ["Expected"] = "Failure => Error != None",
                        ["Actual"] = $"isSuccess={isSuccess}, Errors=NONE",
                        ["Error.isNone"] = true,
                        ["Violation"] = "No errors resulting from failure"
                    }
                );
            }

            IsSuccess = isSuccess;
            Errors = errorList.AsReadOnly();
        }

        public static Result Success() => new(true, Array.Empty<Error>());

        public static Result Failure(Error error) => new(false, new[] { error });

        public static Result Failure(IEnumerable<Error> errors) => new(false, errors);
    }

    public class Result<T> : Result
    {
        private readonly T? _value;

        public T Value => IsSuccess
            ? _value!
            : throw DomainInvalidOperationException.ValueAccessOnFailure<Result>(nameof(Value));

        private Result(T value) : base(true, Array.Empty<Error>()) => _value = value;

        private Result(IEnumerable<Error> errors) : base(false, errors) => _value = default;

        public static Result<T> Success(T value) => new(value);

        public static new Result<T> Failure(Error error) => new(new[] { error });

        public static new Result<T> Failure(IEnumerable<Error> errors) => new(errors);
    }
}
