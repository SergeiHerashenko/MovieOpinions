namespace Authorization.Domain.Common.Validation
{
    /// <summary>
    /// Упорядковує доменні правила за пріоритетом та виконує їх послідовно.
    /// Зупиняє перевірку після першої невдачі.
    ///
    /// (Orders domain validation rules by priority and executes them sequentially.
    /// Stops validation after the first failure.)
    /// </summary>
    /// <typeparam name="TValue">Тип значення, яке перевіряється.</typeparam>
    /// <typeparam name="TFailure">Тип результату невдалої перевірки.</typeparam>
    public sealed class ValidationOrchestrator<TValue, TFailure>
        where TFailure : ValidationFailure
    {
        private readonly IReadOnlyCollection<IValidationRule<TValue, TFailure>> _rules;

        /// <summary>
        /// Створює orchestrator і впорядковує передані правила
        /// за зростанням їхнього пріоритету.
        ///
        /// (Creates the orchestrator and orders the supplied rules
        /// by ascending priority.)
        /// </summary>
        /// <param name="rules">Правила, які необхідно виконати.</param>
        public ValidationOrchestrator(IReadOnlyCollection<IValidationRule<TValue, TFailure>> rules)
        {
            _rules = rules
                .OrderBy(r => (int)r.Priority)
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Виконує правила послідовно до першої невдалої перевірки.
        /// (Executes rules sequentially until the first failure.)
        /// </summary>
        /// <param name="value">Значення для перевірки.</param>
        /// <returns>Перша знайдена помилка або null, якщо всі правила виконані.</returns>
        public TFailure? Validate(TValue value)
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
