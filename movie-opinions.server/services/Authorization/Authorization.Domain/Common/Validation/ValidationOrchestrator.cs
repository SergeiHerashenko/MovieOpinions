using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation.Interfaces;

namespace Authorization.Domain.Common.Validation
{
    /// <summary>
    /// Упорядковує доменні правила за пріоритетом та виконує їх послідовно.
    /// Зупиняє перевірку після першої невдачі.
    /// Правила з однаковим пріоритетом зберігають
    /// початковий порядок переданої колекції.
    ///
    /// (Orders domain validation rules by priority and executes them sequentially.
    /// Stops validation after the first failure.
    /// Rules with the same priority preserve
    /// their original order in the supplied collection.)
    /// </summary>
    /// <typeparam name="TData">Тип даних, які перевіряються.</typeparam>
    /// <typeparam name="TFailure">
    /// Тип представлення виявленого порушення.
    /// </typeparam>
    internal sealed class ValidationOrchestrator<TData, TFailure>
        where TData : class, IValidationData
        where TFailure : class, IValidationFailure
    {
        private readonly IReadOnlyCollection<IValidationRule<TData, TFailure>> _rules;

        /// <summary>
        /// Створює оркестратор і впорядковує передані правила
        /// за зростанням їхнього пріоритету.
        ///
        /// (Creates the orchestrator and orders the supplied rules
        /// by ascending priority.)
        /// </summary>
        /// <param name="rules">Правила, які необхідно виконати.</param>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо колекція правил відсутня або не містить
        /// жодного правила.
        ///
        /// (Thrown when the rule collection is missing or contains
        /// no validation rules.)
        /// </exception>
        public ValidationOrchestrator(IReadOnlyCollection<IValidationRule<TData, TFailure>> rules)
        {
            if(rules is null || rules.Count == 0)
            {
                throw DomainInvalidOperationException.PreconditionFailed<ValidationOrchestrator<TData, TFailure>>(
                    nameof(ValidationOrchestrator<TData, TFailure>),
                    "A non-empty validation rule collection.",
                    OperationType.Create,
                    context: new Dictionary<string, object>
                    {
                        ["RulesIsNull"] = rules is null,
                        ["RuleCount"] = rules?.Count ?? 0
                    }
                );
            }

            _rules = rules
                .OrderBy(r => (int)r.Priority)
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Виконує правила послідовно до першої невдалої перевірки.
        /// 
        /// (Executes rules sequentially until the first failure.)
        /// </summary>
        /// <param name="value">Значення для перевірки.</param>
        /// <returns>
        /// Перше виявлене порушення або null,
        /// якщо всі правила виконані.
        /// </returns>
        public TFailure? Validate(TData value)
        {
            foreach (var rule in _rules)
            {
                var failure = rule.Validate(value);

                if (failure is not null)
                    return failure;
            }

            return null;
        }
    }
}
