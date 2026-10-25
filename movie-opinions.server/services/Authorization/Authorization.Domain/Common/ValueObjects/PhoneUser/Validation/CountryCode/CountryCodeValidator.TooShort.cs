using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Common.Validation.Interfaces;
using Authorization.Domain.Common.ValueObjects.PhoneUser.Errors;

namespace Authorization.Domain.Common.ValueObjects.PhoneUser.Validation.CountryCode
{
    internal static partial class CountryCodeValidator
    {
        /// <summary>
        /// Перевіряє, що загальна довжина міжнародного телефонного коду,
        /// включно із символом '+', становить щонайменше два символи.
        ///
        /// Правило вимагає успішного виконання
        /// <see cref="RequiredRule"/>.
        ///
        /// (Validates that the complete international telephone country code,
        /// including the '+' character, contains at least two characters.
        ///
        /// This rule requires successful validation by
        /// <see cref="RequiredRule"/>.)
        /// </summary>
        private sealed class TooShortCountryCodeRule : IValidationRule<CountryCodeValidationData, ValidationFailure>
        {
            private const int MIN_LENGTH_PHONE_COUNTRY_CODE = 2;

            /// <inheritdoc/>
            public ValidationPriority Priority => ValidationPriority.Length;

            /// <inheritdoc/>
            public ValidationFailure? Validate(CountryCodeValidationData data)
            {
                if (string.IsNullOrWhiteSpace(data.Value))
                {
                    throw DomainInvalidOperationException.PreconditionFailed<PhoneCountryCode>(
                        nameof(TooShortCountryCodeRule),
                        nameof(RequiredRule),
                        data.OperationType,
                        context: new Dictionary<string, object>
                        {
                            ["FieldName"] = nameof(PhoneCountryCode.Value)
                        }
                    );
                }

                if (data.Value.Length >= MIN_LENGTH_PHONE_COUNTRY_CODE)
                    return null;

                return new ValidationFailure()
                {
                    Error = CountryCodeErrors.TooShortCountryCode<PhoneCountryCode>(
                        data.Value.Length,
                        MIN_LENGTH_PHONE_COUNTRY_CODE
                    ),
                    BuildException = operationType =>
                        DomainDataInconsistencyException.ValueOutOfRange<PhoneCountryCode>(
                            nameof(PhoneCountryCode.Value),
                            data.Value.Length,
                            operationType,
                            context: new Dictionary<string, object>
                            {
                                ["ActualLength"] = data.Value.Length,
                                ["MinimumLength"] = MIN_LENGTH_PHONE_COUNTRY_CODE
                            }
                        )
                };
            }
        }
    }
}
