using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.Enums;

namespace Authorization.Domain.Common.Exceptions
{
    /// <summary>
    /// Виняток, що виникає при порушенні консистентності даних у доменному шарі
    /// Exception that occurs when data consistency is violated in the domain layer
    /// </summary>
    public sealed class DomainDataInconsistencyException : BaseDomainException
    {
        private DomainDataInconsistencyException(
            string errorCode,
            ErrorType errorType,
            string message,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
            : base(errorCode, errorType, message, context, innerException) { }

        #region Helpers
        private static Dictionary<string, object> BuildContext<TEntity>(
            OperationType operation = OperationType.Restore,
            IReadOnlyDictionary<string, object>? context = null)
        {
            var data = new Dictionary<string, object>()
            {
                ["entity"] = typeof(TEntity).Name,
                ["operation"] = operation.ToString()
            };
            
            if(context is null)
                return data;

            foreach(var (key, value) in context)
            {
                data[$"user_{key}"] = value;
            }

            return data;
        }
        #endregion

        #region EmptyOnRestore
        /// <summary>
        /// Створює виняток для випадку, коли у агрегаті виявлено пусте значення.
        /// Raises an exception when an empty value is found in the aggregate.
        /// </summary>
        /// <param name="fieldName">Назва параметра, яке викликало проблему.</param>
        /// <typeparam name="TEntity">Назва сутності.</typeparam>
        /// <param name="operationType">Назва операції.</param>
        /// <param name="message">Користувацьке повідомлення. Якщо null – формується стандартне.</param>
        /// <param name="context">Додатковий контекст.</param>
        /// <param name="innerException">Внутрішній виняток.</param>
        public static DomainDataInconsistencyException EmptyOnRestore<TEntity>(
            string fieldName,
            OperationType operationType = OperationType.Restore,
            string? message = null,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            var data = BuildContext<TEntity>(operationType, context);

            data["field"] = fieldName;

            var errorMessage = message ?? BuildEmptyMessage<TEntity>(fieldName);

            return new DomainDataInconsistencyException(
                ErrorCodes.DataInconsistencyError.Inconsistency,
                ErrorType.EmptyValue,
                errorMessage,
                data,
                innerException
            );
        }

        private static string BuildEmptyMessage<TEntity>(string fieldName)
        {
            return $"Cannot restore entity because identifier {fieldName} is invalid. Entity {typeof(TEntity).Name}!";
        }
        #endregion

        #region InvalidValue
        /// <summary>
        /// Створює виняток для випадку, коли агрегат має невалідне значення.
        /// Raises an exception for the case when the aggregate has an invalid value.
        /// </summary>
        /// <param name="fieldName">Назва параметра, яке викликало проблему.</param>
        /// <typeparam name="TEntity">Назва сутності.</typeparam>
        /// <param name="operation">Назва операції.</param>
        /// <param name="message">Користувацьке повідомлення. Якщо null – формується стандартне.</param>
        /// <param name="context">Додатковий контекст.</param>
        /// <param name="innerException">Внутрішній виняток.</param>
        public static DomainDataInconsistencyException InvalidValue<TEntity>(
            string fieldName,
            object? value = null,
            OperationType operation = OperationType.Restore,
            string? message = null,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            var data = BuildContext<TEntity>(operation, context);

            data["field"] = fieldName;

            if (value is not null)
                data["value"] = value;

            var errorMessage = message ?? BuildInvalidValueMessage<TEntity>(fieldName, value);

            return new DomainDataInconsistencyException(
                ErrorCodes.DataInconsistencyError.InvalidValue,
                ErrorType.BusinessRuleViolation,
                errorMessage,
                data,
                innerException
            );
        }

        private static string BuildInvalidValueMessage<TEntity>(string fieldName, object? value)
        {
            return $"Invalid value '{value}' for field '{fieldName}' in entity '{typeof(TEntity).Name}'!";
        }
        #endregion

        #region UnsupportedDiscriminator
        /// <summary>
        /// Створює виняток для випадку, коли при відновленні отримано непідтримуваний дискримінатор.
        /// Throws an exception when an unsupported discriminator is received during recovery
        /// </summary>
        /// <param name="discriminatorValue">Непідтримуване значення.</param>
        /// <typeparam name="TEntity">Назва сутності.</typeparam>
        /// <param name="message">Користувацьке повідомлення.</param>
        /// <param name="operation">Операція (за замовчуванням "restore").</param>
        /// <param name="context">Додатковий контекст.</param>
        /// <param name="innerException">Внутрішній виняток.</param>
        public static DomainDataInconsistencyException UnsupportedDiscriminator<TEntity>(
            string discriminatorValue,
            OperationType operation = OperationType.Restore,
            string? message = null,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            var data = BuildContext<TEntity>(operation, context);

            data["value"] = discriminatorValue;

            var errorMessage = message ?? BuildUnsupportedMessage<TEntity>(discriminatorValue, operation);

            return new DomainDataInconsistencyException(
                ErrorCodes.DataInconsistencyError.UnsupportedType,
                ErrorType.BusinessRuleViolation,
                errorMessage,
                data,
                innerException
            );
        }

        private static string BuildUnsupportedMessage<TEntity>(string unsupportedValue, OperationType operationType)
        {
            return $"Invalid value {unsupportedValue} during {operationType.ToString()}. Entity {typeof(TEntity).Name}!";
        }
        #endregion

        #region InvalidState
        /// <summary>
        /// Створює виняток для випадку некоректного доменного стану.
        /// Raises an exception in case of invalid domain state.
        /// </summary>
        /// <param name="message">Користувацьке повідомлення.</param>
        /// <typeparam name="TEntity">Назва сутності.</typeparam>
        /// <param name="facts">Поточний або очікуваний стан.</param>
        /// <param name="operation">Операція (за замовчуванням "restore").</param>
        /// <param name="innerException">Внутрішній виняток.</param>
        public static DomainDataInconsistencyException ConsistencyViolation<TEntity>(
            IReadOnlyDictionary<string, object> facts,
            OperationType operation = OperationType.Restore,
            string? message = null,
            Exception? innerException = null)
        {
            var data = BuildContext<TEntity>(operation);

            foreach (var (key, value) in facts)
            {
                data[$"fact_{key}"] = value;
            }

            var errorMessage = message ?? BuildConsistencyViolationMessage<TEntity>();

            return new DomainDataInconsistencyException(
                ErrorCodes.DataInconsistencyError.InvalidState,
                ErrorType.InvariantViolation,
                errorMessage,
                data,
                innerException
            );
        }

        private static string BuildConsistencyViolationMessage<TEntity>()
        {
            return $"Consistency violation detected for entity '{typeof(TEntity).Name}'.";
        }
        #endregion
    }
}