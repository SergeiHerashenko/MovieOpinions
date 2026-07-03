using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.Enums;

namespace Authorization.Domain.Common.Exceptions.DomainException
{
    public sealed class DomainDataInconsistencyException : BaseException
    {
        /// <summary>
        /// Виняток, що виникає при порушенні консистентності даних у доменному шарі
        /// Exception that occurs when data consistency is violated in the domain layer
        /// </summary>
        private DomainDataInconsistencyException(
            string errorCode,
            ErrorType errorType,
            string message,
            IReadOnlyDictionary<string, object> context,
            Exception? innerException = null)
            : base(errorCode, errorType, message, context, innerException) { }

        #region Helpers
        private static Dictionary<string, object> BuildContext<TEntity>(
            string fieldName,
            OperationType operationType = OperationType.Restore,
            IReadOnlyDictionary<string, object>? context = null)
        {
            var data = new Dictionary<string, object>()
            {
                ["Layer"] = "Domain",
                ["Entity"] = typeof(TEntity).Name,
                ["Field"] = fieldName,
                ["Operation"] = operationType.ToString()
            };

            if (context is null)
                return data;

            foreach (var (key, value) in context)
            {
                data[$"Custom_{key}"] = value;
            }

            return data;
        }
        #endregion

        #region Empty
        /// <summary>
        /// Створює виняток для випадку, коли у агрегаті виявлено пусте значення.
        /// Raises an exception when an empty value is found in the aggregate.
        /// </summary>
        /// <typeparam name="TEntity">Назва сутності.</typeparam>
        /// <param name="fieldName">Назва параметра, яке викликало проблему.</param>
        /// <param name="operationType">Назва операції.</param>
        /// <param name="message">Користувацьке повідомлення. Якщо null – формується стандартне.</param>
        /// <param name="context">Додатковий контекст.</param>
        /// <param name="innerException">Внутрішній виняток.</param>
        public static DomainDataInconsistencyException Empty<TEntity>(
            string fieldName,
            OperationType operationType = OperationType.Restore,
            string? message = null,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            var data = BuildContext<TEntity>(fieldName, operationType, context);

            var errorMessage = message ?? BuildEmptyMessage<TEntity>(fieldName, operationType);

            return new(
                DomainErrorCodes.DataInconsistencyErrorCode.Inconsistency,
                ErrorType.MissingData,
                errorMessage,
                data,
                innerException
            );
        }

        private static string BuildEmptyMessage<TEntity>(string fieldName, OperationType operationType)
        {
            return $"An error occurred during the {operationType} operation. The field '{fieldName}' in entity '{typeof(TEntity).Name}' is empty!";
        }
        #endregion

        #region InvalidFieldFormat
        /// <summary>
        /// Створює виняток для випадку, коли агрегат має невалідне значення.
        /// Raises an exception for the case when the aggregate has an invalid value.
        /// </summary>
        /// <typeparam name="TEntity">Назва сутності.</typeparam>
        /// <param name="fieldName">Назва параметра, яке викликало проблему.</param>
        /// <param name="value">Саме значення параметру.</param>
        /// <param name="operationType">Назва операції.</param>
        /// <param name="message">Користувацьке повідомлення. Якщо null – формується стандартне.</param>
        /// <param name="context">Додатковий контекст.</param>
        /// <param name="innerException">Внутрішній виняток.</param>
        public static DomainDataInconsistencyException InvalidFieldFormat<TEntity>(
            string fieldName,
            object? value,
            OperationType operationType = OperationType.Restore,
            string? message = null,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            var data = BuildContext<TEntity>(fieldName, operationType, context);

            data["Value"] = value ?? "null";

            var errorMessage = message ?? BuildInvalidFieldFormatMessage<TEntity>(fieldName, operationType, value);

            return new(
                DomainErrorCodes.DataInconsistencyErrorCode.InvalidFormat,
                ErrorType.InvalidFormat,
                errorMessage,
                data,
                innerException
            );
        }

        private static string BuildInvalidFieldFormatMessage<TEntity>(string fieldName, OperationType operationType, object? value)
        {
            return $"An error occurred during the {operationType} operation. The field '{fieldName}' in entity '{typeof(TEntity).Name}' has an invalid format. Provided value: '{value}'!";
        }
        #endregion

        #region UnsupportedDiscriminator
        /// <summary>
        /// Створює виняток для випадку, коли отримано непідтримуваний дискримінатор.
        /// Raises an exception for the case when an unsupported discriminator is received.
        /// </summary>
        /// <typeparam name="TEntity">Назва сутності.</typeparam>
        /// <param name="fieldName">Назва параметра, яке викликало проблему.</param>
        /// <param name="discriminatorValue">Непідтримуване значення.</param>
        /// <param name="operationType">Операція (за замовчуванням "restore").</param>
        /// <param name="message">Користувацьке повідомлення.</param>
        /// <param name="context">Додатковий контекст.</param>
        /// <param name="innerException">Внутрішній виняток.</param>
        public static DomainDataInconsistencyException UnsupportedDiscriminator<TEntity>(
            string fieldName,
            object discriminatorValue,
            OperationType operationType = OperationType.Restore,
            string? message = null,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            var data = BuildContext<TEntity>(fieldName, operationType, context);

            data["Value"] = discriminatorValue;

            var errorMessage = message ?? BuildUnsupportedMessage<TEntity>(discriminatorValue, operationType);

            return new(
                DomainErrorCodes.DataInconsistencyErrorCode.UnsupportedType,
                ErrorType.UnsupportedType,
                errorMessage,
                data,
                innerException
            );
        }

        private static string BuildUnsupportedMessage<TEntity>(object discriminatorValue, OperationType operationType)
        {
            return $"An error occurred during the {operationType} operation. Unsupported discriminator value '{discriminatorValue.ToString()}' for entity '{typeof(TEntity).Name}'!";
        }
        #endregion

        #region ValueOutOfRange
        /// <summary>
        /// Створює виняток для випадку, коли формат і тип абсолютно правильні, але саме значення є абсурдним.
        /// Raises an exception for the case where the format and type are absolutely correct, but the value itself is absurd.
        /// </summary>
        /// /// <typeparam name="TEntity">Назва сутності.</typeparam>
        /// <param name="fieldName">Назва параметра, яке викликало проблему.</param>
        /// <param name="value">Значення яке вишло за межі.</param>
        /// <param name="operationType">Операція (за замовчуванням "restore").</param>
        /// <param name="message">Користувацьке повідомлення.</param>
        /// <param name="context">Додатковий контекст.</param>
        /// <param name="innerException">Внутрішній виняток.</param>
        public static DomainDataInconsistencyException ValueOutOfRange<TEntity>(
            string fieldName,
            object value,
            OperationType operationType = OperationType.Restore,
            string? message = null,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            var data = BuildContext<TEntity>(fieldName, operationType, context);

            data["Value"] = value;

            var errorMessage = message ?? BuildValueOutOfRangeMessage<TEntity>(fieldName, operationType, value);

            return new(
                DomainErrorCodes.DataInconsistencyErrorCode.OutOfRange,
                ErrorType.OutOfRange,
                errorMessage,
                data,
                innerException
            );
        }

        private static string BuildValueOutOfRangeMessage<TEntity>(string fieldName, OperationType operationType, object value)
        {
            return $"An error occurred during the {operationType} operation. The field '{fieldName}' in entity '{typeof(TEntity).Name}' has a value that is out of acceptable range. Provided value: {value}!";
        }
        #endregion
    }
}