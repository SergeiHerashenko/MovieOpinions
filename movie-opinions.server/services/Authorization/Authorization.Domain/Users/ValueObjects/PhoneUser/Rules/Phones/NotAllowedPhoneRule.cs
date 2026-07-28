using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser.Rules.Phones
{
    public sealed class NotAllowedPhoneRule : IValidationRule<PhoneRuleValidationData, ValidationFailure>
    {
        private const string RussianFederationCountryCode = "+7";

        private static readonly HashSet<char> BannedDomainsPart = new()
        {
            '9', '3', '4', '5', '8'
        };

        public ValidationPriority Priority => ValidationPriority.Presence;

        public ValidationFailure? Validate(PhoneRuleValidationData value)
        {
            if(value.CountryCode == RussianFederationCountryCode)
            {
                if (string.IsNullOrEmpty(value.NationalNumber))
                    return null;

                char firstDigit = value.NationalNumber[0];

                if (BannedDomainsPart.Contains(firstDigit))
                {
                    var fullPhone = value.CountryCode + value.NationalNumber;

                    return new ValidationFailure()
                    {
                        Error = PhoneErrors.NotAllowedPhone<Phone>(fullPhone)
                    };
                }  
            }

            return null;
        }
    }
}
