using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.Entities.UsersRestriction.Errors;

namespace Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction.Validation
{
    internal static partial class RestrictionRuleValidator
    {
        /// <summary>
        /// Перевіряє наявність обов’язкової назви правила обмеження.
        ///
        /// (Validates the presence of the required restriction-rule name.)
        /// </summary>
        private sealed class RequiredNameRule : IValidationRule<RestrictionRuleValidationData, ValidationFailure>
        {
            public ValidationPriority Priority => ValidationPriority.Presence;

            public ValidationFailure? Validate(RestrictionRuleValidationData value)
            {
                if (!string.IsNullOrWhiteSpace(value.Name))
                    return null;

                return new ValidationFailure()
                {
                    Error = RestrictionErrors.EmptyRestrictionName<RestrictionRule>(),
                    BuildException = operationType =>
                        DomainDataInconsistencyException.Empty<RestrictionRule>(
                            nameof(RestrictionRule.Name),
                            operationType
                        )
                };
            }
        }
    }
}
