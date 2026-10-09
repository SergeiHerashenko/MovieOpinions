using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.ValueObjects.PhoneUser.Errors;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser.Validation.CountryCode
{
    internal static partial class CountryCodeValidator
    {
        /// <summary>
        /// Перевіряє, що загальна довжина міжнародного телефонного коду,
        /// включно із символом '+', не перевищує чотирьох символів.
        ///
        /// Правило вимагає успішного виконання
        /// <see cref="RequiredRule"/>.
        ///
        /// (Validates that the complete international telephone country code,
        /// including the '+' character, does not exceed four characters.
        ///
        /// This rule requires successful validation by
        /// <see cref="RequiredRule"/>.)
        /// </summary>
        private sealed class TooLongCountryCodeRule : IValidationRule<CountryCodeValidationData, ValidationFailure>
        {
            private const int MAX_LENGTH_PHONE_COUNTRY_CODE = 4;

            public ValidationPriority Priority => ValidationPriority.Length;

            public ValidationFailure? Validate(CountryCodeValidationData data)
            {
                if (string.IsNullOrWhiteSpace(data.Value))
                {
                    throw DomainInvalidOperationException.PreconditionFailed<PhoneCountryCode>(
                        nameof(TooLongCountryCodeRule),
                        nameof(RequiredRule),
                        data.OperationType,
                        context: new Dictionary<string, object>
                        {
                            ["FieldName"] = nameof(PhoneCountryCode.Value)
                        }
                    );
                }

                if (data.Value.Length <= MAX_LENGTH_PHONE_COUNTRY_CODE)
                    return null;

                return new ValidationFailure()
                {
                    Error = CountryCodeErrors.TooLongCountryCode<PhoneCountryCode>(
                        data.Value.Length,
                        MAX_LENGTH_PHONE_COUNTRY_CODE
                    ),
                    BuildException = operationType =>
                        DomainDataInconsistencyException.ValueOutOfRange<PhoneCountryCode>(
                            nameof(PhoneCountryCode.Value),
                            data.Value.Length,
                            operationType,
                            context: new Dictionary<string, object>
                            {
                                ["ActualLength"] = data.Value.Length,
                                ["MaximumLength"] = MAX_LENGTH_PHONE_COUNTRY_CODE
                            }
                        )
                };
            }
        }
    }
}
