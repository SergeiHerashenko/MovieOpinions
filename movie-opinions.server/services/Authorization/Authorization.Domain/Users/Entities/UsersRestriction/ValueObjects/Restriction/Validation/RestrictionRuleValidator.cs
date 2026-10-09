using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation;

namespace Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction.Validation
{
    /// <summary>
    /// Координує повний набір правил валідації RestrictionRule
    /// та формує потрібне представлення виявленої помилки.
    ///
    /// (Coordinates the complete RestrictionRule validation rule set
    /// and produces the required representation of a detected failure.)
    /// </summary>
    internal static partial class RestrictionRuleValidator
    {
        private static readonly ValidationOrchestrator<
            RestrictionRuleValidationData,
            ValidationFailure> _validation = new(
                [
                    new RequiredNameRule(),
                    new MinDurationMinutesRule()
                ]
            );

        /// <summary>
        /// Перевіряє вхідні дані та повертає очікувану доменну помилку.
        ///
        /// (Validates input data and returns an expected domain error.)
        /// </summary>
        /// <param name="operationType">Операція, під час якої виконується валідація.</param>
        /// <param name="name">Назва правила обмеження.</param>
        /// <param name="durationMinutes">Тривалість обмеження у хвилинах.</param>
        /// <returns>Обгортка з помилкою валідації або null.</returns>
        internal static ValidationRulesFailure<Error>? ValidateForError(
            OperationType operationType,
            string name,
            int durationMinutes)
        {
            var data = BuildData(
                operationType,
                name,
                durationMinutes
            );

            return DomainValidationExecutor.ValidateForError(
                _validation,
                data
            );
        }

        /// <summary>
        /// Перевіряє відновлені дані та створює відповідний доменний виняток.
        /// Метод повертає об’єкт винятку, але самостійно його не кидає.
        ///
        /// (Validates restored data and creates the corresponding domain exception.
        /// The method returns the exception object but does not throw it.)
        /// </summary>
        /// <param name="operationType">Операція, для якої створюється виняток.</param>
        /// <param name="name">Відновлена назва правила.</param>
        /// <param name="durationMinutes">Відновлена тривалість у хвилинах.</param>
        /// <returns>Обгортка зі створеним винятком або null.</returns>
        internal static ValidationRulesFailure<Exception>? ValidateForException(
            OperationType operationType,
            string name,
            int durationMinutes)
        {
            var data = BuildData(
                operationType,
                name,
                durationMinutes
            );

            return DomainValidationExecutor.ValidateForException(
                _validation,
                data
            );
        }

        private static RestrictionRuleValidationData BuildData(
            OperationType operationType,
            string name,
            int durationMinutes)
        {
            return new RestrictionRuleValidationData()
            {
                OperationType = operationType,
                Name = name,
                DurationMinutes = durationMinutes
            };
        }
    }
}
