using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.ValueObjects.PhoneUser.Errors;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser.Validation.NationalNumber
{
    internal static partial class NationalNumberValidator
    {
        /// <summary>
        /// Перевіряє, що національний телефонний номер містить
        /// не більше дванадцяти ASCII-цифр.
        ///
        /// Символи форматування не враховуються під час
        /// визначення довжини номера.
        ///
        /// Правило вимагає успішного виконання
        /// <see cref="RequiredRule"/>.
        ///
        /// (Validates that a national telephone number contains
        /// no more than twelve ASCII digits.
        ///
        /// Formatting characters are excluded when determining
        /// the number length.
        ///
        /// This rule requires successful validation by
        /// <see cref="RequiredRule"/>.)
        /// </summary>
        private sealed class TooLongNationalNumberRule
            : IValidationRule<NationalNumberValidationData, ValidationFailure>
        {
            private const int MAX_LENGTH_PHONE_NATIONAL_NUMBER = 12;

            public ValidationPriority Priority => ValidationPriority.Length;

            public ValidationFailure? Validate(NationalNumberValidationData data)
            {
                if (string.IsNullOrWhiteSpace(data.Value))
                {
                    throw DomainInvalidOperationException.PreconditionFailed<PhoneNationalNumber>(
                        nameof(TooLongNationalNumberRule),
                        nameof(RequiredRule),
                        data.OperationType,
                        context: new Dictionary<string, object>
                        {
                            ["FieldName"] = nameof(PhoneNationalNumber.Value)
                        }
                    );
                }

                int digitsCount = data.Value.Count(char.IsAsciiDigit);

                if (digitsCount <= MAX_LENGTH_PHONE_NATIONAL_NUMBER)
                    return null;

                return new ValidationFailure()
                {
                    Error = NationalNumberErrors.TooLongNationalNumber<PhoneNationalNumber>(
                        digitsCount,
                        MAX_LENGTH_PHONE_NATIONAL_NUMBER
                    ),
                    BuildException = operationType =>
                        DomainDataInconsistencyException.ValueOutOfRange<PhoneNationalNumber>(
                            nameof(PhoneNationalNumber.Value),
                            digitsCount,
                            operationType,
                            context: new Dictionary<string, object>
                            {
                                ["ActualDigitCount"] = digitsCount,
                                ["MaximumDigitCount"] = MAX_LENGTH_PHONE_NATIONAL_NUMBER
                            }
                        )
                };
            }
        }
    }
}
