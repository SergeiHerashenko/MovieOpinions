using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction.Rules
{
    public sealed class MinDurationMinutesRule : IValidationRule<RestrictionRuleValidationData, ValidationRestoreFailure>
    {
        public ValidationPriority Priority => ValidationPriority.Format;

        public ValidationRestoreFailure? Validate(RestrictionRuleValidationData value)
        {
            if (string.IsNullOrWhiteSpace(value.Name))
                return null;

            if (value.DurationMinutes > 0)
                return null;

            return new ValidationRestoreFailure()
            {
                Error = RestrictionErrors.InvalidNumberMinutes<RestrictionRule>(),
                BuildException = () => DomainDataInconsistencyException.ValueOutOfRange<RestrictionRule>(nameof(value.DurationMinutes), value.DurationMinutes)
            };
        }
    }
}
