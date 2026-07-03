using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.Enums;

namespace Authorization.Domain.Common.Exceptions.DomainException
{
    public sealed class DomainInvariantViolationException : BaseException
    {
        private DomainInvariantViolationException(
            string errorCode,
            ErrorType errorType,
            string message,
            IReadOnlyDictionary<string, object> context,
            Exception? innerException = null)
            : base(errorCode, errorType, message, context, innerException) { }

        #region BrokenState
        /// <summary>
        /// Створює виняток для випадку, коли стан є критично невалідним.
        /// Raises an exception for the case when the condition is critically invalid.
        /// </summary>
        /// <typeparam name="TEntity">Тип сутності, стан якої порушено.</typeparam>
        /// <param name="ruleDescription">Опис бізнес-правила, яке було порушено.</param>
        /// <param name="stateContext">Зліпок стану (набір полів та їх значень), які викликали конфлікт.</param>
        /// <param name="operationType">Назва операції.</param>
        /// <param name="innerException">Внутрішній виняток.</param>
        public static DomainInvariantViolationException BrokenState<TEntity>(
            string ruleDescription,
            IReadOnlyDictionary<string, object?> stateContext,
            OperationType operationType = OperationType.Restore,
            string? message = null,
            Exception? innerException = null)
        {
            var data = new Dictionary<string, object>()
            {
                ["Layer"] = "Domain",
                ["Entity"] = typeof(TEntity).Name,
                ["Operation"] = operationType.ToString()
            };

            foreach(var (key, value) in stateContext)
            {
                data[$"State_{key}"] = value ?? "null";
            }

            var errorMessage = message ?? BuildBrokenStateMessage<TEntity>(ruleDescription);

            return new(
                DomainErrorCodes.InvariantViolationErrorCode.InvalidState,
                ErrorType.InvariantViolation,
                errorMessage,
                data,
                innerException
            );
        }

        private static string BuildBrokenStateMessage<TEntity>(string ruleDescription)
        {
            return $"Critical invariant violation in entity '{typeof(TEntity).Name}'. Broken rule: {ruleDescription}!";
        }
        #endregion
    }
}