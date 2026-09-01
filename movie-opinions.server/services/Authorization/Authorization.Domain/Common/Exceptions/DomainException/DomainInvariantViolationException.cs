using Authorization.Domain.Common.Exceptions.Enums;

namespace Authorization.Domain.Common.Exceptions.DomainException
{
    /// <summary>
    /// Виняток, що виникає при критичному порушенні інваріанта доменного агрегату або сутності.
    /// 
    /// (Exception that occurs upon a critical violation of a domain aggregate or entity invariant.)
    /// </summary>
    public sealed class DomainInvariantViolationException : BaseException
    {
        private DomainInvariantViolationException(
            string exceptionCode,
            ExceptionType exceptionType,
            string message,
            IReadOnlyDictionary<string, object> context,
            Exception? innerException = null)
            : base(exceptionCode, exceptionType, message, context, innerException) { }

        #region BrokenState
        /// <summary>
        /// Створює виняток для випадку, коли стан є критично невалідним.
        /// 
        /// (Raises an exception for the case when the condition is critically invalid.)
        /// </summary>
        /// <typeparam name="TType">Тип агрегату або сутності, інваріант якої порушено.</typeparam>
        /// <param name="ruleDescription">Опис бізнес-правила, яке було порушено.</param>
        /// <param name="stateContext">Зліпок стану, який демонструє порушення інваріанта.</param>
        /// <param name="operationType">Назва операції.</param>
        /// <param name="message">Власне діагностичне повідомлення. Якщо null — формується стандартне.</param>
        /// <param name="innerException">Внутрішній виняток.</param>
        public static DomainInvariantViolationException BrokenState<TType>(
            string ruleDescription,
            IReadOnlyDictionary<string, object?> stateContext,
            OperationType operationType = OperationType.Restore,
            string? message = null,
            Exception? innerException = null)
        {
            var data = new Dictionary<string, object>()
            {
                ["Layer"] = "Domain",
                ["Type"] = typeof(TType).Name,
                ["Operation"] = operationType.ToString(),
                ["Rule"] = ruleDescription
            };

            foreach (var (key, value) in stateContext)
            {
                data[$"State_{key}"] = value ?? "null";
            }

            var errorMessage = message ?? BuildBrokenStateMessage<TType>(ruleDescription);

            return new(
                DomainExceptionCodes.DomainInvariantViolation.InvalidState,
                ExceptionType.InvariantViolation,
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
