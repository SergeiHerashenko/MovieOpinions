namespace Authorization.Domain.Common.Validation
{
    public sealed class ValidationOrchestrator<TValue, TFailure>
        where TFailure : ValidationFailure
    {
        private readonly IReadOnlyCollection<IValidationRule<TValue, TFailure>> _rules;

        public ValidationOrchestrator(IReadOnlyCollection<IValidationRule<TValue, TFailure>> rules)
        {
            _rules = rules
                .OrderBy(r => (int)r.Priority)
                .ToList()
                .AsReadOnly();
        }

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
