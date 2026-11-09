using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Common.Validation.Interfaces
{
    /// <summary>
    /// Контракт окремого доменного правила валідації.
    /// Повертає опис виявленого порушення або null,
    /// якщо правило виконано.
    ///
    /// (Contract for an individual domain validation rule.
    /// Returns a description of a detected violation,
    /// or null when the rule succeeds.)
    /// </summary>
    /// <typeparam name="TData">
    /// Тип даних, які перевіряються.
    ///
    /// (The type of the data being validated.)
    /// </typeparam>
    /// <typeparam name="TFailure">
    /// Тип представлення виявленого порушення.
    ///
    /// (The representation type of a detected violation.)
    /// </typeparam>
    internal interface IValidationRule<TData, TFailure>
        where TData : class, IValidationData
        where TFailure : class, IValidationFailure
    {
        /// <summary>
        /// Пріоритет, який визначає порядок виконання правила.
        ///
        /// (Priority that determines the rule execution order.)
        /// </summary>
        ValidationPriority Priority { get; }

        /// <summary>
        /// Перевіряє передані дані відповідно до правила.
        ///
        /// (Validates the supplied data against the rule.)
        /// </summary>
        /// <param name="data">
        /// Дані для перевірки.
        ///
        /// (The data to validate.)
        /// </param>
        /// <returns>
        /// Опис виявленого порушення або null,
        /// якщо правило виконано.
        ///
        /// (A description of the detected violation,
        /// or null when the rule succeeds.)
        /// </returns>
        TFailure? Validate(TData data);
    }
}
