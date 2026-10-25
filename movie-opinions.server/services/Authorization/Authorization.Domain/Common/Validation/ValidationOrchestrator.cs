using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation.Interfaces;

namespace Authorization.Domain.Common.Validation
{
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
        /// <param name="rules">
        /// Непорожня колекція правил для виконання.
        /// 
        /// (A non-empty collection of rules to execute.)
        /// </param>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо колекція правил відсутня або не містить
        /// жодного правила.
        ///
        /// (Thrown when the rule collection is missing or contains
        /// no validation rules.)
        /// </exception>
        public ValidationOrchestrator(IReadOnlyCollection<IValidationRule<TData, TFailure>> rules)
        {
            if (rules is null || rules.Count == 0)
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
        /// <param name="value">
        /// Дані для перевірки.
        /// 
        /// (The data to validate.)
        /// </param>
        /// <returns>
        /// Перше виявлене порушення або null,
        /// якщо всі правила виконані.
        /// 
        /// (The first detected violation, or null
        /// when all rules succeed.)
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
