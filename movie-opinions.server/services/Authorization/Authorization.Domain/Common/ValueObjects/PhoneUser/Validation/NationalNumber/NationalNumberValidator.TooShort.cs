using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Common.Validation.Interfaces;
using Authorization.Domain.Common.ValueObjects.PhoneUser.Errors;

namespace Authorization.Domain.Common.ValueObjects.PhoneUser.Validation.NationalNumber
{
    internal static partial class NationalNumberValidator
    {
        /// <summary>
        /// Перевіряє, що національний телефонний номер містить
        /// щонайменше чотири ASCII-цифри.
        ///
        /// Символи форматування не враховуються під час визначення
        /// довжини номера. Правило вимагає успішного виконання
        /// <see cref="RequiredRule"/>.
        ///
        /// (Validates that a national telephone number contains
        /// at least four ASCII digits.
        ///
        /// Formatting characters are excluded when determining the number
        /// length. This rule requires successful validation by
        /// <see cref="RequiredRule"/>.)
        /// </summary>
        private sealed class TooShortNationalNumberRule
            : IValidationRule<NationalNumberValidationData, ValidationFailure>
        {
            private const int MIN_LENGTH_PHONE_NATIONAL_NUMBER = 4;

            /// <inheritdoc/>
            public ValidationPriority Priority => ValidationPriority.Length;

            /// <inheritdoc/>
            public ValidationFailure? Validate(NationalNumberValidationData data)
            {
                if (string.IsNullOrWhiteSpace(data.Value))
                {
                    throw DomainInvalidOperationException.PreconditionFailed<PhoneNationalNumber>(
                        nameof(TooShortNationalNumberRule),
                        nameof(RequiredRule),
                        data.OperationType,
                        context: new Dictionary<string, object>
                        {
                            ["FieldName"] = nameof(PhoneNationalNumber.Value)
                        }
                    );
                }

                int digitsCount = data.Value.Count(char.IsAsciiDigit);

                if (digitsCount >= MIN_LENGTH_PHONE_NATIONAL_NUMBER)
                    return null;

                return new ValidationFailure()
                {
                    Error = NationalNumberErrors.TooShortNationalNumber<PhoneNationalNumber>(
                        digitsCount,
                        MIN_LENGTH_PHONE_NATIONAL_NUMBER
                    ),
                    BuildException = operationType =>
                        DomainDataInconsistencyException.ValueOutOfRange<PhoneNationalNumber>(
                            nameof(PhoneNationalNumber.Value),
                            digitsCount,
                            operationType,
                            context: new Dictionary<string, object>
                            {
                                ["ActualDigitCount"] = digitsCount,
                                ["MinimumDigitCount"] = MIN_LENGTH_PHONE_NATIONAL_NUMBER
                            }
                        )
                };
            }
        }
    }
}
