using Authorization.Domain.Common.Exceptions.Enums;

namespace Authorization.Domain.Common.Exceptions.DomainException
{
    /// <summary>
    /// Виняток, що описує критичне порушення інваріанта доменного об’єкта.
    ///
    /// (Exception describing a critical violation of a domain object's invariant.)
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
        /// Створює виняток, що описує порушення інваріанта доменного об’єкта.
        ///
        /// (Creates an exception describing a domain invariant violation.)
        /// </summary>
        /// <typeparam name="TType">
        /// Тип доменного об’єкта, інваріант якого порушено.
        /// 
        /// (The type of the domain object whose invariant was violated.)
        /// </typeparam>
        /// <param name="ruleDescription">
        /// Опис порушеного інваріанта.
        /// 
        /// (A description of the violated invariant.)
        /// </param>
        /// <param name="stateContext">
        /// Діагностичні дані стану. Ключі додаються до контексту
        /// з префіксом "State_".
        /// Значення null зберігаються як рядок "null".
        /// 
        /// (Diagnostic state data. Keys are added to the context
        /// with the "State_" prefix.
        /// Null values are stored as the string "null".)
        /// </param>
        /// <param name="operationType">
        /// Операція, під час якої виявлено порушення.
        /// За замовчуванням — Restore.
        /// 
        /// (The operation during which the violation was detected.
        /// Defaults to Restore.)
        /// </param>
        /// <param name="message">
        /// Діагностичне повідомлення. Якщо null, формується стандартне.
        /// 
        /// (The diagnostic message. If null, a default message is generated.)
        /// </param>
        /// <param name="innerException">
        /// Початковий виняток, що спричинив проблему, якщо він є.
        /// 
        /// (The original exception that caused the issue, if any.)
        /// </param>
        /// <returns>
        /// Створений виняток порушення доменного інваріанта.
        /// 
        /// (The created domain invariant violation exception.)
        /// </returns>
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
            return $"Critical invariant violation in type '{typeof(TEntity).Name}'. Broken rule: {ruleDescription}.";
        }
        #endregion
    }
}
