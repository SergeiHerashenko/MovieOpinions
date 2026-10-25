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
        /// Перевіряє, що значення починається із символу '+',
        /// а решта символів є ASCII-цифрами.
        /// Перша цифра після '+', якщо вона присутня, не може бути нулем.
        ///
        /// Наявність цифр та допустима довжина перевіряються
        /// окремими правилами довжини.
        ///
        /// Правило вимагає успішного виконання
        /// <see cref="RequiredRule"/>.
        ///
        /// (Validates that the value begins with '+'
        /// and all remaining characters are ASCII digits.
        /// The first digit after '+', when present, must not be zero.
        ///
        /// The presence of digits and the permitted length are checked
        /// by separate length rules.
        ///
        /// This rule requires successful validation by
        /// <see cref="RequiredRule"/>.)
        /// </summary>
        private sealed class FormatCountryCodeRule : IValidationRule<CountryCodeValidationData, ValidationFailure>
        {
            /// <inheritdoc/>
            public ValidationPriority Priority => ValidationPriority.Format;

            /// <inheritdoc/>
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
            /// <param name="value">
            /// Значення, що перевіряється.
            ///
            /// (The value to validate.)
            /// </param>
            /// <returns>
            /// true, якщо формат коректний; інакше false.
            ///
            /// (true if the value satisfies the format rule; otherwise, false.)
            /// </returns>
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
            /// <param name="value">
            /// Некоректне значення телефонного коду.
            ///
            /// (The invalid country calling code.)
            /// </param>
            /// <returns>
            /// Опис виявленого порушення.
            ///
            /// (The description of the detected validation failure.)
            /// </returns>
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
