using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.DomainException;

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
        /// <param name="isSuccess">Ознака успішного виконання.</param>
        /// <param name="errors">Помилки, пов’язані з результатом.</param>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо успішний результат містить помилки
        /// або невдалий результат не містить жодної помилки.
        /// </exception>
        protected Result(
            bool isSuccess,
            IEnumerable<Error> errors)
        {
            var errorList = errors.ToList();

            if (isSuccess && errorList.Any())
            {
                throw DomainInvariantViolationException.BrokenState<Result>(
                    "A successful result cannot contain errors!",
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
                    }
                );
            }

            if (!isSuccess && !errorList.Any())
            {
                throw DomainInvariantViolationException.BrokenState<Result>(
                    "A failed result must contain at least one error!",
                    new Dictionary<string, object?>
                    {
                        ["IsSuccess"] = isSuccess,
                        ["Expected"] = "Failure => Error != None",
                        ["Actual"] = $"isSuccess={isSuccess}, Errors=NONE",
                        ["Error.isNone"] = true,
                        ["Violation"] = "A failed result was created without errors"
                    }
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
        /// <returns>Успішний результат.</returns>
        public static Result Success() => new(
            true,
            Array.Empty<Error>()
        );

        /// <summary>
        /// Створює невдалий результат з однією помилкою.
        ///
        /// (Creates a failed result containing one error.)
        /// </summary>
        /// <param name="error">Помилка, що описує причину невдачі.</param>
        /// <returns>Невдалий результат.</returns>
        public static Result Failure(Error error) => new(
            false,
            new[] { error }
        );

        /// <summary>
        /// Створює невдалий результат із колекцією помилок.
        ///
        /// (Creates a failed result containing a collection of errors.)
        /// </summary>
        /// <param name="errors">Помилки, що описують причини невдачі.</param>
        /// <returns>Невдалий результат.</returns>
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
    /// <typeparam name="T">Тип значення успішного результату.</typeparam>
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
        /// <param name="value">Значення успішного результату.</param>
        /// <returns>Успішний результат із значенням.</returns>
        public static Result<T> Success(T value)
            => new(value);

        /// <summary>
        /// Створює невдалий результат з однією помилкою.
        ///
        /// (Creates a failed result containing one error.)
        /// </summary>
        /// <param name="error">Помилка, що описує причину невдачі.</param>
        /// <returns>Невдалий generic-result.</returns>
        public static new Result<T> Failure(Error error)
            => new(new[] { error });

        /// <summary>
        /// Створює невдалий результат із колекцією помилок.
        ///
        /// (Creates a failed result containing a collection of errors.)
        /// </summary>
        /// <param name="errors">Помилки, що описують причини невдачі.</param>
        /// <returns>Невдалий generic-result.</returns>
        public static new Result<T> Failure(IEnumerable<Error> errors)
            => new(errors);
    }
}
