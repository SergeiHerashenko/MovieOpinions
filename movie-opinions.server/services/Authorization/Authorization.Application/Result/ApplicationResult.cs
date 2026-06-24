using Authorization.Application.Common.Exceptions;
using Authorization.Domain.Common.Errors;

namespace Authorization.Application.Result
{
    public class ApplicationResult
    {
        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public IReadOnlyList<Error> Errors { get; }

        protected ApplicationResult(bool isSuccess, IReadOnlyList<Error> errors)
        {
            if(isSuccess && errors.Any())
            {
                throw ApplicationInvalidInputException.InvariantViolation(
                    "The result of successful execution contains unexpected errors!",
                    new Dictionary<string, object>
                    {
                        ["layer"] = "Application",
                        ["state"] = "Success",
                        ["expected"] = "errors must be empty",
                        ["actual_errors_count"] = errors.Count,
                        ["actual_errors"] = errors.Select(e => new
                        {
                            e.Code,
                            e.Message,
                            e.Type
                        }).ToList(),
                        ["violation"] = "Successful application result must not contain validation or business errors"
                    }
                );
            }

            if(!isSuccess && !errors.Any())
            {
                throw ApplicationInvalidInputException.InvariantViolation(
                    "Failure result must have at least one error",
                    new Dictionary<string, object>
                    {
                        ["layer"] = "Application",
                        ["state"] = "Failure",
                        ["expected"] = "at least one error",
                        ["actual_errors_count"] = 0,
                        ["violation"] = "Failed application result without errors is invalid state"
                    }
                );
            }

            IsSuccess = isSuccess;
            Errors = errors;
        }

        public static ApplicationResult Success() => new(true, Array.Empty<Error>());

        public static ApplicationResult Failure(IReadOnlyList<Error> errors) => new(false, errors);

        public static ApplicationResult Failure(Error error) => new(false, new[] { error });
    }

    public class ApplicationResult<T> : ApplicationResult
    {
        private readonly T? _value;

        public T Value => IsSuccess
            ? _value!
            : throw ApplicationInvalidOperationException.ValueAccessOnFailure(
                "Cannot access value of a failed result!",
                new Dictionary<string, object>
                {
                    ["layer"] = "Application",
                    ["state"] = "Failure",
                    ["expected"] = "IsSuccess == true",
                    ["actual"] = "IsSuccess == false",
                    ["errors"] = Errors.Select(e => new
                    {
                        e.Code,
                        e.Message,
                        e.Type
                    }).ToList(),
                    ["violation"] = "Invalid access of ApplicationResult<T>.Value"
                }
            );

        private ApplicationResult(T? value) : base(true, Array.Empty<Error>())
            => _value = value;

        private ApplicationResult(IReadOnlyList<Error> errors) : base(false, errors)
            => _value = default;

        public static ApplicationResult<T> Success(T value) => new(value);

        public static new ApplicationResult<T> Failure(IReadOnlyList<Error> errors) => new(errors);

        public static new ApplicationResult<T> Failure(Error error) => new(new[] { error });
    }
}