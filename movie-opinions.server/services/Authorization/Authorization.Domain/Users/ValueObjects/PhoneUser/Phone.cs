using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects.PhoneUser.Rules.Phones;
using System.Text.Json.Serialization;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser
{
    public sealed class Phone : ValueObject
    {
        public PhoneCountryCode PhoneCountryCode { get; }

        public PhoneNationalNumber PhoneNationalNumber { get; }

        [JsonConstructor]
        private Phone(
            PhoneCountryCode phoneCountryCode,
            PhoneNationalNumber phoneNationalNumber)
        {
            PhoneCountryCode = phoneCountryCode;
            PhoneNationalNumber = phoneNationalNumber;
        }

        private static readonly ValidationOrchestrator<PhoneRuleValidationData, ValidationFailure> _validator = new(
            [
                new NotAllowedPhoneRule()
            ]
        );

        #region Creation
        public static Result<Phone> Create(PhoneCountryCode phoneCountryCode, PhoneNationalNumber phoneNationalNumber)
        {
            if (phoneCountryCode is null)
                return Result<Phone>.Failure(PhoneErrors.EmptyCountryCode<Phone>());

            if(phoneNationalNumber is null)
                return Result<Phone>.Failure(PhoneErrors.EmptyNationalNumber<Phone>());

            var validationPhone = _validator.Validate(BuildValidationData(phoneCountryCode.Value, phoneNationalNumber.Value));

            if (validationPhone is not null)
                return Result<Phone>.Failure(validationPhone.Error);

            return Result<Phone>.Success(new Phone(phoneCountryCode, phoneNationalNumber));
        }
        #endregion

        #region Restoration
        public static Phone Restore(PhoneCountryCode phoneCountryCode, PhoneNationalNumber phoneNationalNumber)
        {
            DomainGuard.AgainstNull<Phone>(
                (phoneCountryCode, nameof(phoneCountryCode)),
                (phoneNationalNumber, nameof(phoneNationalNumber))
            );

            return new Phone(phoneCountryCode, phoneNationalNumber);
        }
        #endregion

        private static PhoneRuleValidationData BuildValidationData(string countryCode, string nationalNamber)
        {
            return new()
            {
                CountryCode = countryCode,
                NationalNumber = nationalNamber
            };
        }

        public string GetFullNumber()
        {
            return $"{PhoneCountryCode.Value}{PhoneNationalNumber.Value}";
        }

        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return PhoneCountryCode;
            yield return PhoneNationalNumber;
        }
    }
}
