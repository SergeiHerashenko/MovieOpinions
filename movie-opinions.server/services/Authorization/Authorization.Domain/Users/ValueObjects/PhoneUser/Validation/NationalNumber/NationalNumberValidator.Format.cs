using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.ValueObjects.PhoneUser.Errors;
using System.Text.RegularExpressions;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser.Validation.NationalNumber
{
    internal static partial class NationalNumberValidator
    {
        /// <summary>
        /// Перевіряє, що зовнішнє представлення національного номера
        /// містить лише ASCII-цифри, пробільні символи, дефіси та дужки.
        ///
        /// Правило перевіряє лише допустимий набір символів
        /// і не контролює розташування або баланс символів форматування.
        ///
        /// Правило вимагає успішного виконання
        /// <see cref="RequiredRule"/>.
        ///
        /// (Validates that an external national-number representation
        /// contains only ASCII digits, whitespace, hyphens, and parentheses.
        ///
        /// The rule validates only the permitted character set and does not
        /// enforce the placement or balancing of formatting characters.
        ///
        /// This rule requires successful validation by
        /// <see cref="RequiredRule"/>.)
        /// </summary>
        private sealed class FormatNationalNumberRule
            : IValidationRule<NationalNumberValidationData, ValidationFailure>
        {
            /// <summary>
            /// Описує набір символів, дозволених у зовнішньому
            /// представленні національного номера.
            ///
            /// (Defines the character set permitted in an external
            /// national-number representation.)
            /// </summary>
            private static readonly Regex AllowedSymbolsRegex = new(
                @"^[0-9\s\-\(\)]+$", RegexOptions.Compiled
            );

            public ValidationPriority Priority => ValidationPriority.Format;

            public ValidationFailure? Validate(NationalNumberValidationData data)
            {
                if (string.IsNullOrWhiteSpace(data.Value))
                {
                    throw DomainInvalidOperationException.PreconditionFailed<PhoneNationalNumber>(
                        nameof(FormatNationalNumberRule),
                        nameof(RequiredRule),
                        data.OperationType,
                        context: new Dictionary<string, object>
                        {
                            ["FieldName"] = nameof(PhoneNationalNumber.Value)
                        }
                    );
                }

                if (!AllowedSymbolsRegex.IsMatch(data.Value))
                {
                    return new ValidationFailure()
                    {
                        Error = NationalNumberErrors.InvalidFormatNationalNumber<PhoneNationalNumber>(),
                        BuildException = operationType =>
                            DomainDataInconsistencyException.InvalidFieldFormat<PhoneNationalNumber>(
                                nameof(PhoneNationalNumber.Value),
                                data.Value,
                                operationType
                            )
                    };
                }

                return null;
            }
        }
    }
}
