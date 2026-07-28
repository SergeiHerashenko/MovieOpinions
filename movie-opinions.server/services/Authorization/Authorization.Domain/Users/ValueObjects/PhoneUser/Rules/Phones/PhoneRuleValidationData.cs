namespace Authorization.Domain.Users.ValueObjects.PhoneUser.Rules.Phones
{
    public class PhoneRuleValidationData
    {
        public required string CountryCode { get; set; }

        public required string NationalNumber { get; set; }
    }
}
