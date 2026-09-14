using Authorization.Domain.Common.Exceptions.Enums;

namespace Authorization.Domain.Common.Exceptions.DomainException
{
    /// <summary>
    /// Виняток, що виникає при порушенні консистентності даних у доменному шарі.
    /// 
    /// (Exception that occurs when data consistency is violated in the domain layer.)
    /// </summary>
    public sealed class DomainDataInconsistencyException : BaseException
    {
        private DomainDataInconsistencyException(
            string exceptionCode,
            ExceptionType exceptionType,
            string message,
            IReadOnlyDictionary<string, object> context,
            Exception? innerException = null)
            : base(exceptionCode, exceptionType, message, context, innerException) { }

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
        /// Створює виняток, коли в доменному об’єкті виявлено порожнє обов’язкове поле.
        ///
        /// (Creates an exception when an empty required field is found in a domain object.)
        /// </summary>
        /// <typeparam name="TType">Тип доменного об’єкта, в якому виявлено проблему.</typeparam>
        /// <param name="fieldName">Назва поля, яке викликало проблему.</param>
        /// <param name="operationType">Операція (за замовчуванням "Restore").</param>
        /// <param name="message">Діагностичне повідомлення. Якщо null – формується стандартне.</param>
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

            var errorMessage = message ?? BuildEmptyMessage<TType>(
                fieldName,
                operationType
            );

            return new(
                DomainExceptionCodes.DomainDataInconsistency.EmptyValue,
                ExceptionType.DataInconsistency,
                errorMessage,
                data,
                innerException
            );
        }

        private static string BuildEmptyMessage<TType>(string fieldName, OperationType operationType)
        {
            return $"An error occurred during the '{operationType}' operation. The field '{fieldName}' " +
                $"in type '{typeof(TType).Name}' is empty!";
        }
        #endregion

        #region InvalidFieldFormat
        /// <summary>
        /// Створює виняток, коли значення поля доменного об’єкта має невалідний формат.
        ///
        /// (Creates an exception when a domain object field has an invalid format.)
        /// </summary>
        /// <typeparam name="TType">Тип доменного об’єкта, в якому виявлено проблему.</typeparam>
        /// <param name="fieldName">Назва поля, яке викликало проблему.</param>
        /// <param name="value">Значення, тип якого буде додано до діагностичного контексту.</param>
        /// <param name="operationType">Операція (за замовчуванням "Restore").</param>
        /// <param name="message">Діагностичне повідомлення. Якщо null – формується стандартне.</param>
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

            data["ValueType"] = value?.GetType().Name ?? "null";

            var errorMessage = message ?? BuildInvalidFieldFormatMessage<TType>(
                fieldName,
                operationType
            );

            return new(
                DomainExceptionCodes.DomainDataInconsistency.InvalidFormat,
                ExceptionType.DataInconsistency,
                errorMessage,
                data,
                innerException
            );
        }

        private static string BuildInvalidFieldFormatMessage<TType>(string fieldName, OperationType operationType)
        {
            return $"An error occurred during the '{operationType}' operation. The field '{fieldName}' in " +
                $"type '{typeof(TType).Name}' has an invalid format!";
        }
        #endregion

        #region UnsupportedDiscriminator
        /// <summary>
        /// Створює виняток для випадку, коли отримано непідтримуваний дискримінатор.
        /// 
        /// (Raises an exception for the case when an unsupported discriminator is received.)
        /// </summary>
        /// <typeparam name="TType">Тип доменного об’єкта, в якому виявлено проблему.</typeparam>
        /// <param name="fieldName">Назва поля, яке викликало проблему.</param>
        /// <param name="discriminatorValue">Непідтримуване значення.</param>
        /// <param name="operationType">Операція (за замовчуванням "Restore").</param>
        /// <param name="message">Діагностичне повідомлення. Якщо null — формується стандартне.</param>
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

            data["Value"] = discriminatorValue ?? "Unknown";

            var errorMessage = message ?? BuildUnsupportedMessage<TType>(
                discriminatorValue ?? "Unknown",
                operationType
            );

            return new(
                DomainExceptionCodes.DomainDataInconsistency.UnsupportedType,
                ExceptionType.DataInconsistency,
                errorMessage,
                data,
                innerException
            );
        }

        private static string BuildUnsupportedMessage<TType>(object discriminatorValue, OperationType operationType)
        {
            return $"An error occurred during the '{operationType}' operation. Unsupported discriminator " +
                $"value '{discriminatorValue.ToString()}' for type '{typeof(TType).Name}'!";
        }
        #endregion

        #region ValueOutOfRange
        /// <summary>
        /// Створює виняток, коли значення поля виходить за допустимий діапазон.
        ///
        /// (Creates an exception when a field value is outside its allowed range.)
        /// </summary>
        /// <typeparam name="TType">Тип доменного об’єкта, в якому виявлено проблему.</typeparam>
        /// <param name="fieldName">Назва поля, яке викликало проблему.</param>
        /// <param name="value">Значення, тип якого буде додано до діагностичного контексту.</param>
        /// <param name="operationType">Операція (за замовчуванням "Restore").</param>
        /// <param name="message">Діагностичне повідомлення. Якщо null — формується стандартне.</param>
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

            data["ValueType"] = value?.GetType().Name ?? "null";

            var errorMessage = message ?? BuildValueOutOfRangeMessage<TType>(
                fieldName,
                operationType
            );

            return new(
                DomainExceptionCodes.DomainDataInconsistency.OutOfRange,
                ExceptionType.DataInconsistency,
                errorMessage,
                data,
                innerException
            );
        }

        private static string BuildValueOutOfRangeMessage<TType>(string fieldName, OperationType operationType)
        {
            return $"An error occurred during the '{operationType}' operation. The field '{fieldName}' " +
                $"in type '{typeof(TType).Name}' has a value that is out of acceptable range!";
        }
        #endregion
    }
}
