using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Common.Validation
{
    public interface IValidationRule<TValue, TFailure>
        where TFailure : ValidationFailure
    {
        ValidationPriority Priority { get; }

        TFailure? Validate(TValue value);
    }
}
