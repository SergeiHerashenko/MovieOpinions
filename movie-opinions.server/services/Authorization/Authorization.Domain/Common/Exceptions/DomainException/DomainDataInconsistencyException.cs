using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Results;

namespace Authorization.Domain.Common.Exceptions.DomainException
{
    public sealed class DomainDataInconsistencyException : BaseException
    {
        /// <summary>
        /// Виняток, що виникає при порушенні консистентності даних у доменному шарі.
        /// (Exception that occurs when data consistency is violated in the domain layer.)
        /// </summary>
        private DomainDataInconsistencyException(
            string errorCode,
            ErrorType errorType,
            string message,
            IReadOnlyDictionary<string, object> context,
            Exception? innerException = null)
            : base(errorCode, errorType, message, context, innerException) { }

        #region Helpers
        private static Dictionary<string, object> BuildContext<TType>(
            string fieldName,
            OperationType operationType = OperationType.Restore,
            IReadOnlyDictionary<string, object>? context = null)
        {
            var data = new Dictionary<string, object>()
            {
                ["Layer"] = "Domain",
                ["Type"] = typeof(TType).Name,
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
        /// (Raises an exception when an empty value is found in the aggregate.)
        /// </summary>
        /// <typeparam name="TType">Назва типу.</typeparam>
        /// <param name="fieldName">Назва параметра, яке викликало проблему.</param>
        /// <param name="operationType">Назва операції.</param>
        /// <param name="message">Користувацьке повідомлення. Якщо null – формується стандартне.</param>
        /// <param name="context">Додатковий контекст.</param>
        /// <param name="innerException">Внутрішній виняток.</param>
        public static DomainDataInconsistencyException Empty<TType>(
            string fieldName,
            OperationType operationType = OperationType.Restore,
            string? message = null,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            var data = BuildContext<TType>(fieldName, operationType, context);

            var errorMessage = message ?? BuildEmptyMessage<TType>(fieldName, operationType);

            return new(
                DomainErrorCodes.Data.EmptyValue,
                ErrorType.EmptyValue,
                errorMessage,
                data,
                innerException
            );
        }

        private static string BuildEmptyMessage<TType>(string fieldName, OperationType operationType)
        {
            return $"An error occurred during the {operationType} operation. The field '{fieldName}' in type '{typeof(TType).Name}' is empty!";
        }
        #endregion

        #region InvalidFieldFormat
        /// <summary>
        /// Створює виняток для випадку, коли агрегат має невалідне значення.
        /// (Raises an exception for the case when the aggregate has an invalid value.)
        /// </summary>
        /// <typeparam name="TType">Назва типу.</typeparam>
        /// <param name="fieldName">Назва параметра, яке викликало проблему.</param>
        /// <param name="value">Саме значення параметру.</param>
        /// <param name="operationType">Назва операції.</param>
        /// <param name="message">Користувацьке повідомлення. Якщо null – формується стандартне.</param>
        /// <param name="context">Додатковий контекст.</param>
        /// <param name="innerException">Внутрішній виняток.</param>
        public static DomainDataInconsistencyException InvalidFieldFormat<TType>(
            string fieldName,
            object? value,
            OperationType operationType = OperationType.Restore,
            string? message = null,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            var data = BuildContext<TType>(fieldName, operationType, context);

            data["Value"] = value ?? "null";

            var errorMessage = message ?? BuildInvalidFieldFormatMessage<TType>(fieldName, operationType, value);

            return new(
                DomainErrorCodes.Data.InvalidFormat,
                ErrorType.InvalidFormat,
                errorMessage,
                data,
                innerException
            );
        }

        private static string BuildInvalidFieldFormatMessage<TType>(string fieldName, OperationType operationType, object? value)
        {
            return $"An error occurred during the {operationType} operation. The field '{fieldName}' in type '{typeof(TType).Name}' has an invalid format. Provided value: '{value}'!";
        }
        #endregion

        #region UnsupportedDiscriminator
        /// <summary>
        /// Створює виняток для випадку, коли отримано непідтримуваний дискримінатор.
        /// (Raises an exception for the case when an unsupported discriminator is received.)
        /// </summary>
        /// <typeparam name="TType">Назва типу.</typeparam>
        /// <param name="fieldName">Назва параметра, яке викликало проблему.</param>
        /// <param name="discriminatorValue">Непідтримуване значення.</param>
        /// <param name="operationType">Операція (за замовчуванням "restore").</param>
        /// <param name="message">Користувацьке повідомлення.</param>
        /// <param name="context">Додатковий контекст.</param>
        /// <param name="innerException">Внутрішній виняток.</param>
        public static DomainDataInconsistencyException UnsupportedDiscriminator<TType>(
            string fieldName,
            object? discriminatorValue,
            OperationType operationType = OperationType.Restore,
            string? message = null,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            var data = BuildContext<TType>(fieldName, operationType, context);

            data["Value"] = discriminatorValue ?? "Unknow";

            var errorMessage = message ?? BuildUnsupportedMessage<TType>(discriminatorValue ?? "Unknow", operationType);

            return new(
                DomainErrorCodes.Data.UnsupportedType,
                ErrorType.UnsupportedType,
                errorMessage,
                data,
                innerException
            );
        }

        private static string BuildUnsupportedMessage<TType>(object discriminatorValue, OperationType operationType)
        {
            return $"An error occurred during the {operationType} operation. Unsupported discriminator value '{discriminatorValue.ToString()}' for type '{typeof(TType).Name}'!";
        }
        #endregion

        #region ValueOutOfRange
        /// <summary>
        /// Створює виняток для випадку, коли формат і тип абсолютно правильні, але саме значення є абсурдним.
        /// (Raises an exception for the case where the format and type are absolutely correct, but the value itself is absurd.)
        /// </summary>
        /// /// <typeparam name="TType">Назва типу.</typeparam>
        /// <param name="fieldName">Назва параметра, яке викликало проблему.</param>
        /// <param name="value">Значення яке вишло за межі.</param>
        /// <param name="operationType">Операція (за замовчуванням "restore").</param>
        /// <param name="message">Користувацьке повідомлення.</param>
        /// <param name="context">Додатковий контекст.</param>
        /// <param name="innerException">Внутрішній виняток.</param>
        public static DomainDataInconsistencyException ValueOutOfRange<TType>(
            string fieldName,
            object? value = null,
            OperationType operationType = OperationType.Restore,
            string? message = null,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            var data = BuildContext<TType>(fieldName, operationType, context);

            data["Value"] = value ?? "null";

            var errorMessage = message ?? BuildValueOutOfRangeMessage<TType>(fieldName, operationType, value);

            return new(
                DomainErrorCodes.Data.OutOfRange,
                ErrorType.OutOfRange,
                errorMessage,
                data,
                innerException
            );
        }

        private static string BuildValueOutOfRangeMessage<TType>(string fieldName, OperationType operationType, object? value)
        {
            return $"An error occurred during the {operationType} operation. The field '{fieldName}' in type '{typeof(TType).Name}' has a value that is out of acceptable range. Provided value: {value}!";
        }
        #endregion
    }
}
