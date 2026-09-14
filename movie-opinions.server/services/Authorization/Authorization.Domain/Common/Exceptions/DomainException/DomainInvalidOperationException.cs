using Authorization.Domain.Common.Exceptions.Enums;

namespace Authorization.Domain.Common.Exceptions.DomainException
{
    /// <summary>
    /// Виняток, що виникає при спробі виконати недопустиму операцію у доменному шарі.
    /// 
    /// (Exception that occurs when an attempt is made to perform an invalid operation in the domain layer.)
    /// </summary>
    public sealed class DomainInvalidOperationException : BaseException
    {
        private DomainInvalidOperationException(
            string exceptionCode,
            ExceptionType exceptionType,
            string message,
            IReadOnlyDictionary<string, object> context,
            Exception? innerException = null)
            : base(exceptionCode, exceptionType, message, context, innerException) { }

        #region ValueAccessOnFailure
        /// <summary>
        /// Створює виняток для випадку, коли здійснюється спроба отримати значення,
        /// доступ до якого неможливий у поточному стані системи.
        /// 
        /// (Creates an exception for cases when an attempt is made to access a value that is
        /// unavailable in the current state.)
        /// </summary>
        /// <typeparam name="TType">
        /// Тип, у якому виконується невалідна операція.
        /// </typeparam>
        /// <param name="valueName">
        /// Назва значення або властивості, до якої намагалися отримати доступ.
        /// </param>
        /// <param name="operationType">
        /// Тип операції, під час якої стався збій (за замовчуванням Read).
        /// </param>
        /// <param name="message">
        /// Діагностичне повідомлення про помилку. Якщо null – формується стандартне повідомлення.
        /// </param>
        /// <param name="context">
        /// Додатковий контекст із даними стану для структурованого логування.
        /// </param>
        /// <param name="innerException">
        /// Внутрішній виняток, який став першопричиною збою.
        /// </param>
        public static DomainInvalidOperationException ValueAccessOnFailure<TType>(
            string valueName,
            OperationType operationType = OperationType.Read,
            string? message = null,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            var data = new Dictionary<string, object>()
            {
                ["Layer"] = "Domain",
                ["Type"] = typeof(TType).Name,
                ["Operation"] = operationType.ToString(),
                ["ValueName"] = valueName
            };

            if (context is not null)
            {
                foreach (var (key, value) in context)
                {
                    data[$"Custom_{key}"] = value;
                }
            }

            var errorMessage = message ?? BuildValueAccessOnFailureMessage<TType>(valueName);

            return new(
                DomainExceptionCodes.DomainInvalidOperation.ValueAccessOnFailure,
                ExceptionType.InvalidOperation,
                errorMessage,
                data,
                innerException
            );
        }

        private static string BuildValueAccessOnFailureMessage<TType>(string valueName)
        {
            return $"Unable to access the value '{valueName}' in type '{typeof(TType).Name}'!";
        }
        #endregion

        #region NullCallback
        /// <summary>
        /// Створює виняток для випадку, коли переданий колбек (функція зворотного виклику) виявився null.
        /// 
        /// (Creates an exception for cases when a provided callback delegate is null.)
        /// </summary>
        /// <typeparam name="TType">Тип, у якому виконується невалідна операція.</typeparam>
        /// <param name="callbackName">Назва колбеку, який виявився null.</param>
        /// <param name="operationType">Тип операції, під час якої стався збій (за замовчуванням Read).</param>
        /// <param name="message">Діагностичне повідомлення. Якщо null — формується стандартне.</param>
        /// <param name="context">Додатковий контекст із даними стану для структурованого логування.</param>
        /// <param name="innerException">Внутрішній виняток, який став першопричиною збою.</param>
        public static DomainInvalidOperationException NullCallback<TType>(
            string callbackName,
            OperationType operationType = OperationType.Read,
            string? message = null,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            var data = new Dictionary<string, object>()
            {
                ["Layer"] = "Domain",
                ["Type"] = typeof(TType).Name,
                ["Operation"] = operationType.ToString(),
                ["CallbackName"] = callbackName
            };

            if (context is not null)
            {
                foreach (var (key, value) in context)
                {
                    data[$"Custom_{key}"] = value;
                }
            }

            var errorMessage = message ?? BuildNullCallbackMessage<TType>(
                callbackName,
                operationType
            );

            return new(
                DomainExceptionCodes.DomainInvalidOperation.NullCallback,
                ExceptionType.InvalidOperation,
                errorMessage,
                data,
                innerException
            );
        }

        private static string BuildNullCallbackMessage<TType>(string callbackName, OperationType operationType)
        {
            return $"The callback '{callbackName}' is null during {operationType} for type '{typeof(TType).Name}'!";
        }
        #endregion
    }
}
