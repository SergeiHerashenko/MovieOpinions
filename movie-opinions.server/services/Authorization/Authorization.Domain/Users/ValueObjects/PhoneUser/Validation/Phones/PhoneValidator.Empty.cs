using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.ValueObjects.PhoneUser.Errors;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser.Validation.Phones
{
    internal static partial class PhoneValidator
    {
        /// <summary>
        /// Перевіряє наявність міжнародного коду країни
        /// та національної частини телефонного номера.
        ///
        /// Правило виконується першим і встановлює передумову
        /// для політик, що використовують обидві складові.
        ///
        /// (Validates the presence of the international country code
        /// and national part of a telephone number.
        ///
        /// This rule executes first and establishes the prerequisite
        /// for policies that use both components.)
        /// </summary>
        private sealed class RequiredRule : IValidationRule<PhoneValidationData, ValidationFailure>
        {
            public ValidationPriority Priority => ValidationPriority.Presence;

            public ValidationFailure? Validate(PhoneValidationData data)
            {
                var isCountryCodeEmpty = string.IsNullOrWhiteSpace(data.CountryCode);
                var isNationalNumberEmpty = string.IsNullOrWhiteSpace(data.NationalNumber);

                return (isCountryCodeEmpty, isNationalNumberEmpty) switch
                {
                    (true, true) => new ValidationFailure()
                    {
                        Error = PhoneErrors.EmptyPhone<Phone>(),
                        BuildException = operationType =>
                            DomainDataInconsistencyException.Empty<Phone>(
                                nameof(Phone),
                                operationType
                            )
                    },
                    (true, false) => new ValidationFailure()
                    {
                        Error = CountryCodeErrors.EmptyCountryCode<Phone>(),
                        BuildException = operationType =>
                            DomainDataInconsistencyException.Empty<Phone>(
                                nameof(Phone.PhoneCountryCode),
                                operationType
                            )
                    },
                    (false, true) => new ValidationFailure()
                    {
                        Error = NationalNumberErrors.EmptyNationalNumber<Phone>(),
                        BuildException = operationType =>
                            DomainDataInconsistencyException.Empty<Phone>(
                                nameof(Phone.PhoneNationalNumber),
                                operationType
                            )
                    },
                    (false, false) => null
                };
            }
        }
    }
}
