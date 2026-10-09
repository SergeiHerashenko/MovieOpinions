using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Validation.Interfaces;

namespace Authorization.Domain.Common.Validation
{
    /// <summary>
    /// Виконує доменну валідацію через переданий оркестратор
    /// і перетворює виявлене порушення на потрібний тип результату:
    /// очікувану доменну помилку або об’єкт винятку.
    ///
    /// Винятки, безпосередньо кинуті правилами через порушення
    /// внутрішніх передумов, не перехоплюються.
    ///
    /// (Executes domain validation through the supplied orchestrator
    /// and converts a detected violation into the required result type:
    /// an expected domain error or an exception object.
    ///
    /// Exceptions thrown directly by rules because of violated internal
    /// preconditions are not intercepted.)
    /// </summary>
    internal static class DomainValidationExecutor
    {
        /// <summary>
        /// Виконує валідацію та повертає очікувану доменну помилку.
        ///
        /// (Executes validation and returns an expected domain error.)
        /// </summary>
        /// <typeparam name="TData">Тип даних, що перевіряються.</typeparam>
        /// <typeparam name="TFailure">
        /// Тип представлення порушення, що містить очікувану
        /// доменну помилку.
        /// </typeparam>
        /// <param name="validator">Оркестратор правил валідації.</param>
        /// <param name="data">Дані для перевірки.</param>
        /// <returns>
        /// Обгортка з першою виявленою доменною помилкою
        /// або null, якщо всі правила виконані.
        /// </returns>
        internal static ValidationRulesFailure<Error>? ValidateForError<TData, TFailure>(
            ValidationOrchestrator<TData, TFailure> validator,
            TData data)
            where TData : class, IValidationData
            where TFailure : class, IValidationFailure
        {
            var failure = validator.Validate(data);

            if (failure is null)
                return null;

            return new ValidationRulesFailure<Error>(failure.Error);
        }

        /// <summary>
        /// Виконує валідацію та створює відповідний об’єкт винятку
        /// для виявленого порушення, але самостійно його не кидає.
        ///
        /// (Executes validation and creates the corresponding exception
        /// object for a detected violation without throwing it.)
        /// </summary>
        /// <typeparam name="TData">
        /// Тип даних, що перевіряються та містять контекст операції.
        /// </typeparam>
        /// <typeparam name="TFailure">
        /// Тип представлення порушення, що підтримує
        /// побудову доменного винятку.
        /// </typeparam>
        /// <param name="validator">Оркестратор правил валідації.</param>
        /// <param name="data">
        /// Дані для перевірки разом із типом виконуваної операції.
        /// </param>
        /// <returns>
        /// Обгортка зі створеним об’єктом винятку
        /// або null, якщо всі правила виконані.
        /// </returns>
        internal static ValidationRulesFailure<Exception>? ValidateForException<TData, TFailure>(
            ValidationOrchestrator<TData, TFailure> validator,
            TData data)
            where TData : class, IHasOperationType
            where TFailure : class, IExceptionValidationFailure
        {
            var failure = validator.Validate(data);

            if (failure is null)
                return null;

            var exception = failure.BuildException(data.OperationType);

            return new ValidationRulesFailure<Exception>(exception);
        }
    }
}
