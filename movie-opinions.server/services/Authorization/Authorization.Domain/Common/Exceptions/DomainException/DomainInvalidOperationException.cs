using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.Enums;

namespace Authorization.Domain.Common.Exceptions.DomainException
{
    public sealed class DomainInvalidOperationException : BaseException
    {
        private DomainInvalidOperationException(
            string errorCode,
            ErrorType errorType,
            string message,
            IReadOnlyDictionary<string, object> context,
            Exception? innerException = null)
            : base(errorCode, errorType, message, context, innerException) { }

        # region ValueAccessOnFailure
        /// <summary>
        /// Створює виняток для випадку, коли здійснюється спроба отримати значення, доступ до якого неможливий у поточному стані системи.
        /// Creates an exception for cases when an attempt is made to access a value that is unavailable in the current state.
        /// </summary>
        /// <typeparam name="TEntity">Тип сутності, в якій виконується невалідна операція.</typeparam>
        /// <param name="valueName">Назва значення або властивості, до якої намагалися отримати доступ.</param>
        /// <param name="operationType">Тип операції, під час якої стався збій (за замовчуванням Reading).</param>
        /// <param name="message">Користувацьке повідомлення про помилку. Якщо null – формується стандартне повідомлення.</param>
        /// <param name="context">Додатковий контекст із даними стану для структурованого логування.</param>
        /// <param name="innerException">Внутрішній виняток, який став першопричиною збою.</param>
        public static DomainInvalidOperationException ValueAccessOnFailure<TEntity>(
            string valueName,
            OperationType operationType = OperationType.Reading,
            string? message = null,
            IReadOnlyDictionary<string, object>? context = null,
            Exception? innerException = null)
        {
            var data = new Dictionary<string, object>()
            {
                ["Layer"] = "Domain",
                ["Entity"] = typeof(TEntity).Name,
                ["Operation"] = operationType.ToString(),
                ["Value"] = valueName
            };

            if (context is not null)
            {
                foreach(var(key, value) in context)
                {
                    data[$"Custom_{key}"] = value;
                }
            }

            var errorMessage = message ?? BuildValueAccessOnFailureMessage<TEntity>(valueName);

            return new(
                DomainErrorCodes.InvalidOperationErrorCode.InvalidOperation,
                ErrorType.InvalidOperation,
                errorMessage,
                data,
                innerException
            );
        }

        private static string BuildValueAccessOnFailureMessage<TEntity>(string valueName)
        {
            return $"Unable to access the value '{valueName}'. Entity '{typeof(TEntity).Name}'!";
        }
        #endregion
    }
}
