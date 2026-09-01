using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Common.Validation
{
    /// <summary>
    /// Контракт окремого доменного правила валідації.
    /// Повертає опис помилки при невдачі або null, якщо правило виконано.
    ///
    /// (Contract for an individual domain validation rule.
    /// Returns a failure when validation fails, or null when the rule succeeds.) 
    /// </summary>
    /// <typeparam name="TValue">Тип значення, яке перевіряється.</typeparam>
    /// <typeparam name="TFailure">Тип результату невдалої перевірки.</typeparam>
    public interface IValidationRule<TValue, TFailure>
        where TFailure : ValidationFailure
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
        /// <param name="value">Значення для перевірки.</param>
        /// <returns>
        /// Помилка валідації або null, якщо правило виконано.
        /// </returns>
        TFailure? Validate(TValue value);
    }
}
