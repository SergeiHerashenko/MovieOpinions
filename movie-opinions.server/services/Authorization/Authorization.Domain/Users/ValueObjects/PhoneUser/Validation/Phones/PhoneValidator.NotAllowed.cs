using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.ValueObjects.PhoneUser.Errors;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser.Validation.Phones
{
    internal static partial class PhoneValidator
    {
        /// <summary>
        /// Перевіряє політику заборонених телефонних номерів
        /// на основі міжнародного коду та першої цифри
        /// національної частини.
        ///
        /// Правило застосовується лише під час створення і повинно
        /// виконуватися після успішної перевірки
        /// <see cref="RequiredRule"/>.
        ///
        /// (Validates the blocked telephone-number policy using
        /// the international country code and the first digit
        /// of the national part.
        ///
        /// This rule applies only during creation and must execute
        /// after successful validation by
        /// <see cref="RequiredRule"/>.)
        /// </summary>
        private sealed class NotAllowedPhoneRule : IValidationRule<PhoneValidationData, ValidationFailure>
        {
            /// <summary>
            /// Міжнародний код, для якого застосовується
            /// політика заборонених діапазонів.
            ///
            /// (International calling code to which
            /// the blocked-range policy applies.)
            /// </summary>
            private const string RussianFederationCountryCode = "+7";

            /// <summary>
            /// Перші цифри національної частини, заборонені
            /// для номера з обмеженим міжнародним кодом.
            ///
            /// (First national-number digits blocked
            /// for the restricted international calling code.)
            /// </summary>
            private static readonly HashSet<char> BlockedNationalNumberFirstDigits = new()
            {
                '9', '3', '4', '5', '8'
            };

            public ValidationPriority Priority => ValidationPriority.BusinessRule;

            public ValidationFailure? Validate(PhoneValidationData data)
            {
                if(string.IsNullOrWhiteSpace(data.NationalNumber) || string.IsNullOrWhiteSpace(data.CountryCode))
                {
                    throw DomainInvalidOperationException.PreconditionFailed<Phone>(
                        nameof(NotAllowedPhoneRule),
                        nameof(RequiredRule),
                        data.OperationType,
                        context: new Dictionary<string, object>
                        {
                            ["FieldName"] = nameof(Phone)
                        }
                    );
                }

                if(data.CountryCode == RussianFederationCountryCode)
                {
                    char firstDigit = data.NationalNumber[0];

                    if (BlockedNationalNumberFirstDigits.Contains(firstDigit))
                    {
                        var fullPhone = data.CountryCode + data.NationalNumber;

                        return new ValidationFailure()
                        {
                            Error = PhoneErrors.NotAllowedPhone<Phone>(),
                            BuildException = operationType =>
                                DomainDataInconsistencyException.UnsupportedDiscriminator<Phone>(
                                    nameof(Phone),
                                    fullPhone,
                                    operationType
                                )
                        };
                    }
                }

                return null;
            }
        }
    }
}
