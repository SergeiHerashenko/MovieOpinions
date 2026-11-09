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
        /// Перевіряє наявність обов’язкового значення
        /// національного телефонного номера.
        ///
        /// Правило виконується першим і встановлює передумову
        /// для наступних правил формату та кількості цифр.
        ///
        /// (Validates the presence of the required national
        /// telephone-number value.
        ///
        /// This rule executes first and establishes the prerequisite
        /// for subsequent format and digit-count rules.)
        /// </summary>
        private sealed class RequiredRule : IValidationRule<NationalNumberValidationData, ValidationFailure>
        {
            /// <inheritdoc/>
            public ValidationPriority Priority => ValidationPriority.Presence;

            /// <inheritdoc/>
            public ValidationFailure? Validate(NationalNumberValidationData data)
            {
                if (!string.IsNullOrWhiteSpace(data.Value))
                {
                    return null;
                }

                return new ValidationFailure()
                {
                    Error = NationalNumberErrors.EmptyNationalNumber<PhoneNationalNumber>(),
                    BuildException = operationType =>
                        DomainDataInconsistencyException.Empty<PhoneNationalNumber>(
                            nameof(PhoneNationalNumber.Value),
                            operationType
                        )
                };
            }
        }
    }
}
