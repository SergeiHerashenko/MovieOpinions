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
    /// Тип представлення failure, наприклад Error або Exception.
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

        internal ValidationRulesFailure(TFailure value)
        {
            Value = value;
        }
    }
}
