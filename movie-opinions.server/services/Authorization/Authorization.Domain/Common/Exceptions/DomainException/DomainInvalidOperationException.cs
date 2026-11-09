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
        /// Тип, у якому виконується некоректна операція.
        ///
        /// (The type in which the invalid operation is performed.)
        /// </typeparam>
        /// <param name="valueName">
        /// Назва значення або властивості, до яких намагалися отримати доступ.
        ///
        /// (The name of the value or property that was accessed.)
        /// </param>
        /// <param name="operationType">
        /// Операція, під час якої виявлено проблему.
        /// За замовчуванням — Read.
        ///
        /// (The operation during which the issue was detected.
        /// Defaults to Read.)
        /// </param>
        /// <param name="message">
        /// Діагностичне повідомлення. Якщо null, формується стандартне.
        ///
        /// (The diagnostic message. If null, a default message is generated.)
        /// </param>
        /// <param name="context">
        /// Додаткові дані, ключі яких додаються до контексту
        /// з префіксом "Custom_".
        ///
        /// (Additional data whose keys are added to the context
        /// with the "Custom_" prefix.)
        /// </param>
        /// <param name="innerException">
        /// Початковий виняток, що спричинив проблему, якщо він є.
        ///
        /// (The original exception that caused the issue, if any.)
        /// </param>
        /// <returns>
        /// Створений виняток некоректної операції.
        ///
        /// (The created invalid operation exception.)
        /// </returns>
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
        /// <typeparam name="TType">
        /// Тип, у якому виконується некоректна операція.
        ///
        /// (The type in which the invalid operation is performed.)
        /// </typeparam>
        /// <param name="callbackName">
        /// Назва колбеку, значення якого дорівнює null.
        ///
        /// (The name of the callback whose value is null.)
        /// </param>
        /// <param name="operationType">
        /// Операція, під час якої виявлено проблему.
        /// За замовчуванням — Read.
        ///
        /// (The operation during which the issue was detected.
        /// Defaults to Read.)
        /// </param>
        /// <param name="message">
        /// Діагностичне повідомлення. Якщо null, формується стандартне.
        ///
        /// (The diagnostic message. If null, a default message is generated.)
        /// </param>
        /// <param name="context">
        /// Додаткові дані, ключі яких додаються до контексту
        /// з префіксом "Custom_".
        ///
        /// (Additional data whose keys are added to the context
        /// with the "Custom_" prefix.)
        /// </param>
        /// <param name="innerException">
        /// Початковий виняток, що спричинив проблему, якщо він є.
        ///
        /// (The original exception that caused the issue, if any.)
        /// </param>
        /// <returns>
        /// Створений виняток некоректної операції.
        ///
        /// (The created invalid operation exception.)
        /// </returns>
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

        private static string BuildNullCallbackMessage<TType>(
            string callbackName,
            OperationType operationType)
        {
            return $"The callback '{callbackName}' is null during {operationType} for type '{typeof(TType).Name}'!";
        }
        #endregion

        #region PreconditionFailed
        /// <summary>
        /// Створює виняток для випадку, коли порушено передумову (precondition)
        /// або необхідний стан для виконання операції.
        ///
        /// (Creates an exception for cases when a precondition or required state for executing an
        /// operation is violated.)
        /// </summary>
        /// <typeparam name="TType">
        /// Тип, у якому виконується некоректна операція.
        ///
        /// (The type in which the invalid operation is performed.)
        /// </typeparam>
        /// <param name="sourceContext">
        /// Назва компонента, правила або методу,
        /// у якому виявлено порушення передумови.
        ///
        /// (The name of the component, rule, or method
        /// where the precondition violation was detected.)
        /// </param>
        /// <param name="prerequisite">
        /// Опис умови, яка має виконуватися для здійснення операції.
        ///
        /// (A description of the condition that must hold
        /// for the operation to proceed.)
        /// </param>
        /// <param name="operationType">
        /// Операція, під час якої виявлено проблему.
        /// За замовчуванням — Read.
        ///
        /// (The operation during which the issue was detected.
        /// Defaults to Read.)
        /// </param>
        /// <param name="message">
        /// Діагностичне повідомлення. Якщо null, формується стандартне.
        ///
        /// (The diagnostic message. If null, a default message is generated.)
        /// </param>
        /// <param name="context">
        /// Додаткові дані, ключі яких додаються до контексту
        /// з префіксом "Custom_".
        ///
        /// (Additional data whose keys are added to the context
        /// with the "Custom_" prefix.)
        /// </param>
        /// <param name="innerException">
        /// Початковий виняток, що спричинив проблему, якщо він є.
        ///
        /// (The original exception that caused the issue, if any.)
        /// </param>
        /// <returns>
        /// Створений виняток некоректної операції.
        ///
        /// (The created invalid operation exception.)
        /// </returns>
        public static DomainInvalidOperationException PreconditionFailed<TType>(
            string sourceContext,
            string prerequisite,
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
                ["SourceContext"] = sourceContext,
                ["Prerequisite"] = prerequisite
            };

            if (context is not null)
            {
                foreach (var (key, value) in context)
                {
                    data[$"Custom_{key}"] = value;
                }
            }

            var errorMessage = message ?? BuildPreconditionFailedMessage<TType>(
                sourceContext,
                prerequisite
            );

            return new(
                DomainExceptionCodes.DomainInvalidOperation.PreconditionFailed,
                ExceptionType.InvalidOperation,
                errorMessage,
                data,
                innerException
            );
        }

        private static string BuildPreconditionFailedMessage<TType>(
            string sourceContext,
            string prerequisite)
        {
            return $"Precondition failed in '{sourceContext}' for type '{typeof(TType).Name}'. " +
                $"Required prerequisite '{prerequisite}' was not satisfied.";
        }
        #endregion
    }
}
