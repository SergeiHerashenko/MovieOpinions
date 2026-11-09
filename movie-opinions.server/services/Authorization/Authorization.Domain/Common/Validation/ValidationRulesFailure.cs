namespace Authorization.Domain.Common.Validation
{
    /// <summary>
    /// Містить одне вибране представлення невдалої перевірки
    /// набору правил.
    ///
    /// (Contains one selected representation of a failed
    /// validation-rules evaluation.)
    /// </summary>
    /// <typeparam name="TFailure">
    /// Тип представлення невдалої перевірки, наприклад Error або Exception.
    ///
    /// (The validation failure representation type, such as Error or Exception.)
    /// </typeparam>
    internal sealed class ValidationRulesFailure<TFailure>
        where TFailure : class
    {
        /// <summary>
        /// Вибране представлення невдалої перевірки.
        ///
        /// (Selected representation of the validation failure.)
        /// </summary>
        public TFailure Value { get; }

        /// <summary>
        /// Створює обгортку вибраного представлення невдалої перевірки.
        ///
        /// (Creates a wrapper for the selected validation failure representation.)
        /// </summary>
        /// <param name="value">
        /// Вибране представлення невдалої перевірки.
        ///
        /// (The selected validation failure representation.)
        /// </param>
        internal ValidationRulesFailure(TFailure value)
        {
            Value = value;
        }
    }
}
