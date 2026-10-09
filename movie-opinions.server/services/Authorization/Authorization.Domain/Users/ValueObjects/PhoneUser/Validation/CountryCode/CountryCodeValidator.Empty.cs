using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.ValueObjects.PhoneUser.Errors;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser.Validation.CountryCode
{
    internal static partial class CountryCodeValidator
    {
        /// <summary>
        /// Перевіряє наявність обов’язкового значення
        /// міжнародного телефонного коду.
        ///
        /// Правило виконується першим і встановлює передумову
        /// для наступних правил формату та довжини.
        ///
        /// (Validates the presence of the required international
        /// telephone country-code value.
        ///
        /// This rule executes first and establishes the prerequisite
        /// for subsequent format and length rules.)
        /// </summary>
        private sealed class RequiredRule : IValidationRule<CountryCodeValidationData, ValidationFailure>
        {
            public ValidationPriority Priority => ValidationPriority.Presence;

            public ValidationFailure? Validate(CountryCodeValidationData data)
            {
                if (!string.IsNullOrWhiteSpace(data.Value))
                    return null;

                return new ValidationFailure()
                {
                    Error = CountryCodeErrors.EmptyCountryCode<PhoneCountryCode>(),
                    BuildException = operationType =>
                        DomainDataInconsistencyException.Empty<PhoneCountryCode>(
                            nameof(PhoneCountryCode.Value),
                            operationType
                        )
                };
            }
        }
    }
}
