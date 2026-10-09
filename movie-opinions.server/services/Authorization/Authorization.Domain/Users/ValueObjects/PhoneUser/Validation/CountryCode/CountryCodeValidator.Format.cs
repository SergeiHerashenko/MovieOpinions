using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.ValueObjects.PhoneUser.Errors;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser.Validation.CountryCode
{
    internal static partial class CountryCodeValidator
    {
        /// <summary>
        /// Перевіряє формат міжнародного телефонного коду:
        /// значення повинно починатися із символу '+', містити
        /// лише ASCII-цифри та не починатися з нуля.
        ///
        /// Обмеження кількості цифр перевіряються окремими
        /// правилами довжини.
        ///
        /// Правило вимагає успішного виконання
        /// <see cref="RequiredRule"/>.
        ///
        /// (Validates the international telephone country-code format:
        /// the value must begin with '+', contain only ASCII digits,
        /// and must not begin with zero.
        ///
        /// Digit-count restrictions are validated by separate length rules.
        ///
        /// This rule requires successful validation by
        /// <see cref="RequiredRule"/>.)
        /// </summary>
        private sealed class FormatCountryCodeRule : IValidationRule<CountryCodeValidationData, ValidationFailure>
        {
            public ValidationPriority Priority => ValidationPriority.Format;

            public ValidationFailure? Validate(CountryCodeValidationData data)
            {
                if (string.IsNullOrWhiteSpace(data.Value))
                {
                    throw DomainInvalidOperationException.PreconditionFailed<PhoneCountryCode>(
                        nameof(FormatCountryCodeRule),
                        nameof(RequiredRule),
                        data.OperationType,
                        context: new Dictionary<string, object>
                        {
                            ["FieldName"] = nameof(PhoneCountryCode.Value)
                        }
                    );
                }

                if (HasValidFormat(data.Value))
                    return null;

                return CreateFailure(data.Value);
            }

            /// <summary>
            /// Визначає, чи відповідає значення синтаксичному формату
            /// міжнародного телефонного коду.
            ///
            /// (Determines whether the value conforms to the syntactic format
            /// of an international telephone country code.)
            /// </summary>
            /// <param name="value">Значення, що перевіряється.</param>
            /// <returns>true, якщо формат коректний; інакше false.</returns>
            private static bool HasValidFormat(string value)
            {
                if (!value.StartsWith('+'))
                    return false;

                var digits = value[1..];

                if (!digits.All(char.IsAsciiDigit))
                    return false;

                if (digits.Length > 0 && digits[0] == '0')
                    return false;

                return true;
            }

            /// <summary>
            /// Створює результат порушення правила формату
            /// міжнародного телефонного коду.
            ///
            /// (Creates a validation failure for an invalid international
            /// telephone country-code format.)
            /// </summary>
            /// <param name="value">Некоректне значення телефонного коду.</param>
            /// <returns>Опис виявленого порушення.</returns>
            private static ValidationFailure CreateFailure(string value)
            {
                return new ValidationFailure()
                {
                    Error = CountryCodeErrors.InvalidFormatCountryCode<PhoneCountryCode>(),
                    BuildException = operationType =>
                        DomainDataInconsistencyException.InvalidFieldFormat<PhoneCountryCode>(
                            nameof(PhoneCountryCode.Value),
                            value,
                            operationType
                        )
                };
            }
        }
    }
}
