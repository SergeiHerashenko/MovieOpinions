using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;

namespace Authorization.Domain.Results
{
    /// <summary>
    /// Представляє результат виконання операції без значення:
    /// успішний результат або невдачу з однією чи кількома помилками.
    ///
    /// (Represents the outcome of an operation without a value:
    /// either success or failure with one or more errors.)
    /// </summary>
    /// <remarks>
    /// Успішний результат не може містити помилок,
    /// а невдалий результат повинен містити щонайменше одну помилку.
    ///
    /// (A successful result cannot contain errors,
    /// while a failed result must contain at least one error.)
    /// </remarks>
    public class Result
    {
        /// <summary>
        /// Вказує, чи завершилася операція успішно.
        ///
        /// (Indicates whether the operation completed successfully.)
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Вказує, чи завершилася операція невдало.
        ///
        /// (Indicates whether the operation failed.)
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Помилки, що описують причини невдалого результату.
        /// Для успішного результату колекція завжди порожня.
        ///
        /// (Errors describing the reasons for failure.
        /// The collection is always empty for a successful result.)
        /// </summary>
        public IReadOnlyCollection<Error> Errors { get; }

        /// <summary>
        /// Ініціалізує результат і перевіряє узгодженість
        /// його статусу з переданою колекцією помилок.
        ///
        /// (Initializes a result and validates consistency
        /// between its status and the supplied error collection.)
        /// </summary>
        /// <param name="isSuccess">
        /// Ознака успішного виконання.
        ///
        /// (Indicates whether the operation succeeded.)
        /// </param>
        /// <param name="errors">
        /// Колекція помилок результату без елементів null.
        ///
        /// (The result's error collection containing no null entries.)
        /// </param>
        /// <exception cref="DomainInvariantViolationException">
        /// Успішний результат містить помилки, невдалий результат
        /// не містить жодної помилки або колекція містить null-елементи.
        ///
        /// (A successful result contains errors, a failed result
        /// contains no errors, or the collection contains null entries.)
        /// </exception>
        protected Result(
            bool isSuccess,
            IEnumerable<Error> errors)
        {
            DomainGuard.AgainstNull<Result>(
                OperationType.Create,
                (errors, nameof(errors))
            );

            var errorList = errors.ToList();

            if(errorList.Any(x => x is null))
            {
                throw DomainInvariantViolationException.BrokenState<Result>(
                    "A result's error collection must not contain null entries.",
                    new Dictionary<string, object?>
                    {
                        ["IsSuccess"] = isSuccess,
                        ["ErrorCount"] = errorList.Count,
                        ["NullErrorCount"] = errorList.Count(error => error is null),
                        ["Expected"] = "Every error entry must be a non-null Error instance.",
                        ["Violation"] = "The error collection contains null entries."
                    },
                    OperationType.Create
                );
            }

            if (isSuccess && errorList.Any())
            {
                throw DomainInvariantViolationException.BrokenState<Result>(
                    "A successful result cannot contain errors.",
                    new Dictionary<string, object?>
                    {
                        ["IsSuccess"] = isSuccess,
                        ["Expected"] = "Success => Errors.Count == 0",
                        ["Actual"] = $"isSuccess={isSuccess}, errors.Count={errorList.Count}",
                        ["Errors"] = errorList.Select(e => new
                        {
                            e.Code,
                            e.Message,
                            Type = e.ErrorType.ToString()
                        }).ToArray(),
                        ["Violation"] = "A successful result contains unexpected errors."
                    },
                    OperationType.Create
                );
            }

            if (!isSuccess && !errorList.Any())
            {
                throw DomainInvariantViolationException.BrokenState<Result>(
                    "A failed result must contain at least one error.",
                    new Dictionary<string, object?>
                    {
                        ["IsSuccess"] = isSuccess,
                        ["Expected"] = "Failure => Errors.Count >= 1",
                        ["Actual"] = $"isSuccess={isSuccess}, errors.Count={errorList.Count}",
                        ["Violation"] = "A failed result was created without errors"
                    },
                    OperationType.Create
                );
            }

            IsSuccess = isSuccess;
            Errors = errorList.AsReadOnly();
        }

        /// <summary>
        /// Створює успішний результат без значення.
        ///
        /// (Creates a successful result without a value.)
        /// </summary>
        /// <returns>
        /// Успішний результат.
        ///
        /// (A successful result.)
        /// </returns>
        public static Result Success() => new(
            true,
            Array.Empty<Error>()
        );

        /// <summary>
        /// Створює невдалий результат з однією помилкою.
        ///
        /// (Creates a failed result containing one error.)
        /// </summary>
        /// <param name="error">
        /// Помилка, що описує причину невдачі.
        ///
        /// (The error describing the reason for failure.)
        /// </param>
        /// <returns>
        /// Невдалий результат.
        ///
        /// (A failed result.)
        /// </returns>
        public static Result Failure(Error error) => new(
            false,
            new[] { error }
        );

        /// <summary>
        /// Створює невдалий результат із колекцією помилок.
        ///
        /// (Creates a failed result containing a collection of errors.)
        /// </summary>
        /// <param name="errors">
        /// Непорожня колекція помилок без елементів null.
        ///
        /// (A non-empty error collection containing no null entries.)
        /// </param>
        /// <returns>
        /// Невдалий результат.
        ///
        /// (A failed result.)
        /// </returns>
        public static Result Failure(IEnumerable<Error> errors) => new(
            false,
            errors
        );
    }

    /// <summary>
    /// Представляє результат виконання операції, яка у разі успіху
    /// повертає значення вказаного типу.
    ///
    /// (Represents the outcome of an operation that returns
    /// a value of the specified type on success.)
    /// </summary>
    /// <typeparam name="T">
    /// Тип значення успішного результату.
    ///
    /// (The type of the value carried by a successful result.)
    /// </typeparam>
    public class Result<T> : Result
    {
        private readonly T? _value;

        /// <summary>
        /// Значення успішного результату.
        ///
        /// (Value carried by a successful result.)
        /// </summary>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає під час спроби отримати значення невдалого результату.
        ///
        /// (Occurs when attempting to retrieve the value of a failed result.)
        /// </exception>
        public T Value => IsSuccess
            ? _value!
            : throw DomainInvalidOperationException.ValueAccessOnFailure<Result<T>>(nameof(Value));

        private Result(T value)
            : base(true, Array.Empty<Error>())
        {
            _value = value;
        }

        private Result(IEnumerable<Error> errors)
            : base(false, errors)
        {
            _value = default;
        }

        /// <summary>
        /// Створює успішний результат із заданим значенням.
        ///
        /// (Creates a successful result containing the supplied value.)
        /// </summary>
        /// <param name="value">
        /// Значення успішного результату.
        ///
        /// (The value of the successful result.)
        /// </param>
        /// <returns>
        /// Успішний результат із заданим значенням.
        ///
        /// (A successful result containing the supplied value.)
        /// </returns>
        public static Result<T> Success(T value)
            => new(value);

        /// <summary>
        /// Створює невдалий результат з однією помилкою.
        ///
        /// (Creates a failed result containing one error.)
        /// </summary>
        /// <param name="error">
        /// Помилка, що описує причину невдачі.
        ///
        /// (The error describing the reason for failure.)
        /// </param>
        /// <returns>
        /// Невдалий результат.
        ///
        /// (A failed result.)
        /// </returns>
        public static new Result<T> Failure(Error error)
            => new(new[] { error });

        /// <summary>
        /// Створює невдалий результат із колекцією помилок.
        ///
        /// (Creates a failed result containing a collection of errors.)
        /// </summary>
        /// <param name="errors">
        /// Непорожня колекція помилок без елементів null.
        ///
        /// (A non-empty error collection containing no null entries.)
        /// </param>
        /// <returns>
        /// Невдалий результат.
        ///
        /// (A failed result.)
        /// </returns>
        public static new Result<T> Failure(IEnumerable<Error> errors)
            => new(errors);
    }
}
