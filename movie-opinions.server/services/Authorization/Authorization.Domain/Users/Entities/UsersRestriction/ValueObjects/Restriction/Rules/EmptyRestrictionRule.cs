using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction.Rules
{
    public sealed class EmptyRestrictionRule : IValidationRule<RestrictionRuleValidationData, ValidationRestoreFailure>
    {
        public ValidationPriority Priority => ValidationPriority.Presence;

        public ValidationRestoreFailure? Validate(RestrictionRuleValidationData value)
        {
            if (!string.IsNullOrWhiteSpace(value.Name))
                return null;

            return new ValidationRestoreFailure()
            {
                Error = RestrictionErrors.EmptyNameRestriction<RestrictionRule>(),
                BuildException = () => DomainDataInconsistencyException.Empty<RestrictionRule>(nameof(value))
            };
        }
    }
}
