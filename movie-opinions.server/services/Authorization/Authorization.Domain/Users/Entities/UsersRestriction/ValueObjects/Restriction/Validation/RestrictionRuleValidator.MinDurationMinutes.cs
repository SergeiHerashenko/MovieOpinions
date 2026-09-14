using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.Entities.UsersRestriction.Errors;

namespace Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction.Validation
{
    internal static partial class RestrictionRuleValidator
    {
        /// <summary>
        /// Перевіряє, що тривалість обмеження є додатною кількістю хвилин.
        ///
        /// (Validates that the restriction duration is a positive number of minutes.)
        /// </summary>
        private sealed class MinDurationMinutesRule : IValidationRule<RestrictionRuleValidationData, ValidationFailure>
        {
            public ValidationPriority Priority => ValidationPriority.Format;

            public ValidationFailure? Validate(RestrictionRuleValidationData value)
            {
                if (value.DurationMinutes > 0)
                    return null;

                return new ValidationFailure()
                {
                    Error = RestrictionErrors.InvalidDuration<RestrictionRule>(value.DurationMinutes),
                    BuildException = operationType =>
                        DomainDataInconsistencyException.ValueOutOfRange<RestrictionRule>(
                            nameof(RestrictionRule.DurationMinutes),
                            value.DurationMinutes,
                            operationType
                        )
                };
            }
        }
    }
}
