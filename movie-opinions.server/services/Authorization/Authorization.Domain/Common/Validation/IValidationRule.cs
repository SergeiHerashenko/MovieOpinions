using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Common.Validation.Interfaces;

namespace Authorization.Domain.Common.Validation
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
    /// <typeparam name="TData">Тип даних, які перевіряються.</typeparam>
    /// <typeparam name="TFailure">
    /// Тип представлення виявленого порушення.
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
        /// Перевіряє передане значення відповідно до правила.
        /// 
        /// (Validates the supplied value against the rule.)
        /// </summary>
        /// <param name="data">Значення для перевірки.</param>
        /// <returns>
        /// Опис виявленого порушення або null,
        /// якщо правило виконано.
        /// </returns>
        TFailure? Validate(TData data);
    }
}
